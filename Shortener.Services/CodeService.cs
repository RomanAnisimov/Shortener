using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;
using Shortener.Application.Abstractions.Interfaces;
using Shortener.Application.DTOs;
using Shortener.Application.Options;
using Shortener.Data.Entities;
using Shortener.Services.Mapping;
using Shortener.Shared;
using Shortener.Shared.Events;

namespace Shortener.Services
{
    public class CodeService(
        ICodeRepository codeRepository,
        ILinkCache cache,
        IKafkaProducerService kafkaProducerService,
        IOptions<AppOptions> options)
        : ICodeService
    {
        private const int MaxCodeAttempts = 5;
        private readonly string _baseUrl = options.Value.BaseUrl.TrimEnd('/');

        public async Task<LinkResponse?> GetLinkByCodeAsync(string code, CancellationToken ct)
        {
            var link = await GetLinkEntityAsync(code, ct);
            return link is null ? null : LinkMapper.ToLinkResponse(link, _baseUrl);
        }

        public async Task<LinkResponse> CreateLinkAsync(ShortenRequest request, CancellationToken ct = default)
        {
            if (request.ExpiresAt is not null && request.ExpiresAt <= DateTimeOffset.UtcNow)
                throw new ArgumentException("ExpiresAt must be in the future", nameof(request));

            for (var attempt = 0; attempt < MaxCodeAttempts; attempt++)
            {
                var code = CodeGenerator.Generate();

                var link = new Link
                {
                    Code = code,
                    OriginalUrl = request.Url,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ExpiresAt = request.ExpiresAt
                };

                try
                {
                    await codeRepository.AddLink(link, ct);
                    await cache.SetAsync(link, ct: ct);

                    return LinkMapper.ToLinkResponse(link, _baseUrl);
                }
                catch (DbUpdateException ex) when (IsUniqueCodeViolation(ex)) { }
            }

            throw new InvalidOperationException("Could not generate unique code");
        }

        public async Task<LinkResponse?> HandleClickAsync(string code, CancellationToken ct = default)
        {
            var link = await GetLinkEntityAsync(code, ct);
            if (link == null)
                return null;

            await cache.IncrementClickCountAsync(code, ct);

            var linkClicked = new LinkClicked(code, DateTimeOffset.UtcNow, null, null, null);
            await kafkaProducerService.SendAsync(KafkaTopics.LinkClicked, linkClicked, ct: ct);
            return LinkMapper.ToLinkResponse(link, _baseUrl);
        }

        private async Task<Link?> GetLinkEntityAsync(string code, CancellationToken ct)
        {
            var link = await cache.GetByCodeAsync(code, ct);
            var fromCache = link != null;

            link ??= await codeRepository.GetLinkByCode(code, ct);

            if (link == null)
                return null;

            if (link.ExpiresAt != null && link.ExpiresAt < DateTimeOffset.UtcNow)
            {
                await cache.RemoveAsync(code, ct);
                return null;
            }

            if (!fromCache)
                await cache.SetAsync(link, ct: ct);

            return link;
        }

        private static bool IsUniqueCodeViolation(DbUpdateException ex)
            => ex.InnerException is PostgresException pg
               && pg.SqlState == PostgresErrorCodes.UniqueViolation
               && pg.ConstraintName == "IX_Links_Code";
    }
}
