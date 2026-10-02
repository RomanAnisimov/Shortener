using Microsoft.EntityFrameworkCore;
using Shortener.Application.Abstractions.Interfaces;
using Shortener.Data.Entities;
using Shortener.Shared;
using Shortener.Shared.DTO;
using Shortener.Shared.Events;

namespace Shortener.Services
{
    public class CodeService(
        ICodeRepository codeRepository,
        ILinkCache cache,
        IKafkaProducerService kafkaProducerService)
        : ICodeService
    {
        private const int MaxCodeAttempts = 5;

        public async Task<Link?> GetLinkByCodeAsync(string code)
        {
            var link = await cache.GetByCodeAsync(code);
            var fromCache = link != null;

            link ??= await codeRepository.GetLinkByCode(code);

            if (link == null)
                return null;

            if (link.ExpiresAt != null && link.ExpiresAt < DateTimeOffset.UtcNow)
            {
                await cache.RemoveAsync(code);
                return null;
            }

            if (!fromCache)
                await cache.SetAsync(link);

            return link;
        }

        public async Task<Link> CreateLinkAsync(ShortenRequest request, CancellationToken ct = default)
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
                    await codeRepository.AddLink(link);
                    await cache.SetAsync(link, ct: ct);

                    return link;
                }
                catch (DbUpdateException) {}
            }

            throw new InvalidOperationException("Could not generate unique code");
        }

        public async Task<Link?> HandleClickAsync(string code, CancellationToken ct = default)
        {
            var link = await GetLinkByCodeAsync(code);
            if (link == null)
                return null;

            await cache.IncrementClickCountAsync(code, ct);

            var linkClicked = new LinkClicked(code, DateTimeOffset.UtcNow, null, null, null);
            await kafkaProducerService.SendAsync(KafkaTopics.LinkClicked, linkClicked, ct: ct);
            return link;
        }
    }
}
