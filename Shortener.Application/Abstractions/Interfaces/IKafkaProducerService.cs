namespace Shortener.Application.Abstractions.Interfaces;

public interface IKafkaProducerService
{
    Task SendAsync<T>(string topic, T payload, CancellationToken ct = default);
}