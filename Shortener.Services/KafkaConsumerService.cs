using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Shortener.Application.Abstractions.Interfaces;
using Shortener.Shared;
using Shortener.Shared.Events;
using System.Text.Json;

namespace Shortener.Services;

public class KafkaConsumerService(
    ILogger<KafkaConsumerService> logger,
    ICodeRepository codeRepository) : IKafkaConsumerService
{
    public async Task HandleAsync(ConsumeResult<Ignore, string> result, CancellationToken ct)
    {
        switch (result.Topic)
        {
            case KafkaTopics.LinkClicked:
                var clicked = JsonSerializer.Deserialize<LinkClicked>(result.Message.Value);
                if (clicked is null) return;

                // Пока наивно: +1 к счётчику. Батчинг добавим позже.
                await codeRepository.IncreaseClickCountByCode(clicked.Code, 1);

                logger.LogInformation("Click on {Code}", clicked.Code);
                break;

            case KafkaTopics.LinkCreated:
                var created = JsonSerializer.Deserialize<LinkCreated>(result.Message.Value);
                logger.LogInformation("Link created: {Code}", created?.Code);
                break;
        }
    }
}