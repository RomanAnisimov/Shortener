namespace Shortener.Application.DTOs;

public record ShortenRequest(string Url, DateTimeOffset? ExpiresAt);