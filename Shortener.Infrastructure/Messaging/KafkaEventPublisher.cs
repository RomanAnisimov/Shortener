using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shortener.Application.Abstractions.Interfaces;
using Shortener.Shared.Events;

namespace Shortener.Infrastructure.Messaging;

public class KafkaEventPublisher : IEventPublisher, IDisposable
{
    private readonly IProducer<Null, string> _producer;
    private readonly ILogger<KafkaEventPublisher> _logger;

    public KafkaEventPublisher(
        IConfiguration config,
        ILogger<KafkaEventPublisher> logger)
    {
        _logger = logger;

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = config["Kafka:BootstrapServers"] ?? "localhost:9092",
            Acks = Acks.All,
            EnableIdempotence = true
        };

        _producer = new ProducerBuilder<Null, string>(producerConfig).Build();
    }

    public async Task PublishAsync<T>(T @event, CancellationToken ct = default)
        where T : IDomainEvent
    {
        var topic = TopicResolver.Resolve<T>();
        var json = JsonSerializer.Serialize(@event);

        var result = await _producer.ProduceAsync(
            topic,
            new Message<Null, string> { Value = json },
            ct);

        _logger.LogInformation("Published {EventType} to {Topic} [{Partition}] @ {Offset}",
            typeof(T).Name, result.Topic, result.Partition, result.Offset);
    }

    public void Dispose() => _producer.Dispose();
}