using Confluent.Kafka;

namespace Shortener.Application.Abstractions.Interfaces;

public interface IKafkaConsumerService
{
    Task HandleAsync(ConsumeResult<Ignore, string> result, CancellationToken ct);
}