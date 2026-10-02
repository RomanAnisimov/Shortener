using Shortener.Data.Entities;

namespace Shortener.Application.Abstractions.Interfaces;

public interface ICodeRepository
{
    Task<Link?> GetLinkByCode(string code, CancellationToken ct = default);

    Task<Link> AddLink(Link link, CancellationToken ct = default);

    Task IncreaseClickCountByCode(string code, int i, CancellationToken ct = default);

    Task BulkIncrementClickCountsAsync(Dictionary<string, long> counts, CancellationToken ct = default);
}