using Shortener.Data.Entities;

namespace Shortener.Application.Abstractions.Interfaces;

public interface ILinkCache
{
    Task<Link?> GetByCodeAsync(string code, CancellationToken ct = default);

    Task SetAsync(Link link, TimeSpan? ttl = null, CancellationToken ct = default);

    Task RemoveAsync(string code, CancellationToken ct = default);

    Task<long> IncrementClickCountAsync(string code, CancellationToken ct = default);

    Task<Dictionary<string, long>> GetAndResetClickCountsAsync(IEnumerable<string> codes, CancellationToken ct = default);
}