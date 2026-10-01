using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shortener.Application.Abstractions.Interfaces;
using Shortener.Data;
using Shortener.Shared;
using Shortener.Shared.Events;
using System.Text.Json;

namespace Shortener.Services;

public class KafkaConsumerService : IKafkaConsumerService
{
    private readonly ILogger<KafkaConsumerService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConsumer<Ignore, string> consumer;

    public KafkaConsumerService(
        ILogger<KafkaConsumerService> logger,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public async Task HandleAsync(ConsumeResult<Ignore, string> result, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        switch (result.Topic)
        {
            case KafkaTopics.LinkClicked:
                var clicked = JsonSerializer.Deserialize<LinkClicked>(result.Message.Value);
                if (clicked is null) return;

                // Пока наивно: +1 к счётчику. Батчинг добавим позже.
                await db.Links
                    .Where(l => l.Code == clicked.Code)
                    .ExecuteUpdateAsync(s => s.SetProperty(l => l.ClickCount, l => l.ClickCount + 1), ct);

                _logger.LogInformation("Click on {Code}", clicked.Code);
                break;

            case KafkaTopics.LinkCreated:
                var created = JsonSerializer.Deserialize<LinkCreated>(result.Message.Value);
                _logger.LogInformation("Link created: {Code}", created?.Code);
                break;
        }
    }
}