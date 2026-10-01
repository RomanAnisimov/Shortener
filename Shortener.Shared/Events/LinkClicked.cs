namespace Shortener.Shared.Events;

public record LinkClicked(
    string Code,
    DateTimeOffset ClickedAt,
    string? Referer,
    string? UserAgent,
    string? IpAddress
);