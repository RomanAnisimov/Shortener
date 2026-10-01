using Shortener.Data.Entities;

namespace Shortener.Application.Abstractions.Interfaces;

public interface ICodeRepository
{
    Task<bool> IsCodeExistsAsync(string code);

    Task<Link> AddLink(Link link);
}