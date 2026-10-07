using Shortener.Shared.Events;

namespace Shortener.Application.Abstractions.Interfaces;

public interface IEventPublisher
{
    Task PublishAsync<T>(T @event, CancellationToken ct = default) where T : IDomainEvent;
}