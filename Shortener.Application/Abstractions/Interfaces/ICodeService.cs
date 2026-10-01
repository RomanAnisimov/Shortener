using Shortener.Data.Entities;
using Shortener.Shared.DTO;

namespace Shortener.Application.Abstractions.Interfaces;

public interface ICodeService
{
    Task<Link?> GetLinkByCode(string code);

    Task<Link> GenerateLink(ShortenRequest request);

    Task<Link?> AddLink(Link link);
}