using Shortener.Shared;
using Shortener.Shared.Events;

namespace Shortener.Infrastructure.Messaging;

public static class TopicResolver
{
    public static string Resolve<T>() where T : IDomainEvent
    {
        return typeof(T) switch
        {
            var t when t == typeof(LinkCreated) => KafkaTopics.LinkCreated,
            var t when t == typeof(LinkClicked) => KafkaTopics.LinkClicked,
            _ => throw new InvalidOperationException(
                $"No topic mapping for event type {typeof(T).Name}")
        };
    }
}