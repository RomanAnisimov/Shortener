namespace Shortener.Shared.DTO;

public record ShortenRequest(string Url, DateTimeOffset? ExpiresAt);