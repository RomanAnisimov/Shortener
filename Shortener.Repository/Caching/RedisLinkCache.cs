using Microsoft.Extensions.Logging;
using Shortener.Application.Abstractions.Interfaces;
using Shortener.Data.Entities;
using StackExchange.Redis;
using System.Text.Json;

namespace Shortener.Repository.Caching;

public class RedisLinkCache(
    IConnectionMultiplexer redis,
    ILogger<RedisLinkCache> logger)
    : ILinkCache
{
    private static readonly TimeSpan DefaultTtl = TimeSpan.FromHours(24);
    private static string LinkKey(string code) => $"link:{code}";
    private static string ClickKey(string code) => $"clicks:{code}";

    public async Task<Link?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        var db = redis.GetDatabase();
        var value = await db.StringGetAsync(LinkKey(code));
        if (value.IsNullOrEmpty) return null;

        return JsonSerializer.Deserialize<Link>((string)value!);
    }

    public async Task SetAsync(Link link, TimeSpan? ttl = null, CancellationToken ct = default)
    {
        var db = redis.GetDatabase();
        var json = JsonSerializer.Serialize(link);
        await db.StringSetAsync(LinkKey(link.Code), json, ttl ?? DefaultTtl);
    }

    public async Task RemoveAsync(string code, CancellationToken ct = default)
    {
        var db = redis.GetDatabase();
        await db.KeyDeleteAsync(LinkKey(code));
    }

    public async Task<long> IncrementClickCountAsync(string code, CancellationToken ct = default)
    {
        var db = redis.GetDatabase();
        return await db.StringIncrementAsync(ClickKey(code));
    }
}