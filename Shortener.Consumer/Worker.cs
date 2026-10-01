using Confluent.Kafka;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Shortener.Data;
using Shortener.Shared;
using Shortener.Shared.Events;

namespace Shortener.Consumer
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _config;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConsumer<Ignore, string> _consumer;

        public Worker(ILogger<Worker> logger, IConfiguration config, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _config = config;
            _scopeFactory = scopeFactory;

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = config["Kafka:BootstrapServers"] ?? "localhost:9092",
                GroupId = "shortener-consumer",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };
            _consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.Subscribe(new[] { KafkaTopics.LinkCreated, KafkaTopics.LinkClicked });
            _logger.LogInformation("Consumer started");

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var result = _consumer.Consume(stoppingToken);
                    await HandleAsync(result, stoppingToken);
                    _consumer.Commit(result);
                }
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                _consumer.Close();
            }
        }

        private async Task HandleAsync(ConsumeResult<Ignore, string> result, CancellationToken ct)
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

        public override void Dispose()
        {
            _consumer?.Dispose();
            base.Dispose();
        }
    }
}