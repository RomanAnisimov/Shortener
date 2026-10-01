namespace Shortener.Data.Entities;

public class Link
{
    public long Id { get; set; }
    public string Code { get; set; } = default!;
    public string OriginalUrl { get; set; } = default!;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public long ClickCount { get; set; }
}