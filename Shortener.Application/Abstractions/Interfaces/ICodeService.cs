using Shortener.Application.DTOs;
using Shortener.Data.Entities;

namespace Shortener.Application.Abstractions.Interfaces;

public interface ICodeService
{
    Task<Link?> GetLinkByCodeAsync(string code, CancellationToken ct = default);

    Task<Link> CreateLinkAsync(ShortenRequest request, CancellationToken ct = default);

    Task<Link?> HandleClickAsync(string code, CancellationToken ct = default);
}