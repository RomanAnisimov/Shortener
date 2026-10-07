using Shortener.Application.DTOs;

namespace Shortener.Application.Abstractions.Interfaces;

public interface ICodeService
{
    Task<LinkResponse?> GetLinkByCodeAsync(string code, CancellationToken ct = default);

    Task<LinkResponse> CreateLinkAsync(ShortenRequest request, CancellationToken ct = default);

    Task<LinkResponse?> HandleClickAsync(string code, CancellationToken ct = default);
}