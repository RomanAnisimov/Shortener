using Shortener.Application.Abstractions.Interfaces;
using Shortener.Data.Entities;
using Shortener.Shared.DTO;

namespace Shortener.Services
{
    public class CodeService(
        ICodeRepository codeRepository)
        : ICodeService
    {
        public async Task<Link?> GetLinkByCode(string code)
        {
            return await codeRepository.GetLinkByCode(code);
        }

        public async Task<Link> GenerateLink(ShortenRequest request)
        {
            string code;
            while (true)
            {
                code = CodeGenerator.Generate();
                if (!await codeRepository.IsCodeExistsAsync(code)) break;
            }

            var link = new Link
            {
                Code = code,
                OriginalUrl = request.Url,
                CreatedAt = DateTimeOffset.UtcNow,
                ExpiresAt = request.ExpiresAt
            };

            return link;
        }
    }
}
