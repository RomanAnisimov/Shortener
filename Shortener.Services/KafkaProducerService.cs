using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shortener.Application.Abstractions.Interfaces;

namespace Shortener.Services;

public class KafkaProducerService : IKafkaProducerService, IDisposable
{
    private readonly IProducer<Null, string> _producer;
    private readonly ILogger<KafkaProducerService> _logger;

    public KafkaProducerService(IConfiguration config, ILogger<KafkaProducerService> logger)
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

    public async Task SendAsync<T>(string topic, T payload, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(payload);
        var result = await _producer.ProduceAsync(topic, new Message<Null, string> { Value = json }, ct);
        _logger.LogInformation("Published to {Topic} [{Partition}] @ {Offset}",
            result.Topic, result.Partition, result.Offset);
    }

    public void Dispose() => _producer.Dispose();
}