using Shortener.Data.Entities;
using Shortener.Shared.DTO;

namespace Shortener.Application.Abstractions.Interfaces;

public interface ICodeService
{
    Task<Link?> GetLinkByCodeAsync(string code);

    Task<Link> CreateLinkAsync(ShortenRequest request, CancellationToken ct = default);

    Task<Link?> HandleClickAsync(string code, CancellationToken ct = default);
}