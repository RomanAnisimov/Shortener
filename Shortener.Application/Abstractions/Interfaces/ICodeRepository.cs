using Shortener.Data.Entities;

namespace Shortener.Application.Abstractions.Interfaces;

public interface ICodeRepository
{
    Task<Link?> GetLinkByCode(string code);

    Task<Link> AddLink(Link link);

    Task IncreaseClickCountByCode(string code, int i);
}