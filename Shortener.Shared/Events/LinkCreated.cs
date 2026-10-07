namespace Shortener.Shared.Events;

public record LinkCreated(
    string Code,
    string OriginalUrl,
    DateTimeOffset CreatedAt
) : IDomainEvent;