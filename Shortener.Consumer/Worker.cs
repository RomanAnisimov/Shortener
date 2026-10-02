using System.Text.Json;
using Confluent.Kafka;
using Shortener.Application.Abstractions.Interfaces;
using Shortener.Shared;
using Shortener.Shared.Events;

namespace Shortener.Consumer
{
    public class Worker : BackgroundService
    {
        private const int BatchSize = 100;
        private static readonly TimeSpan FlushInterval = TimeSpan.FromSeconds(5);

        private readonly ILogger<Worker> _logger;
        private readonly IConsumer<Ignore, string> _consumer;
        private readonly IServiceScopeFactory _scopeFactory;


        public Worker(ILogger<Worker> logger,
            IConfiguration config,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
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
            _consumer.Subscribe(new[] { KafkaTopics.LinkClicked });
            _logger.LogInformation("Consumer started");

            var buffer = new List<ConsumeResult<Ignore, string>>(BatchSize);

            try
            {
                var lastFlush = DateTimeOffset.UtcNow;
                while (!stoppingToken.IsCancellationRequested)
                {
                    var result = _consumer.Consume(TimeSpan.FromMilliseconds(500));

                    if (result is not null)
                    {
                        buffer.Add(result);

                        if (buffer.Count >= BatchSize)
                        {
                            await FlushAsync(buffer, stoppingToken);
                            buffer.Clear();
                            lastFlush = DateTimeOffset.UtcNow;
                        }
                    }
                    else if (buffer.Count > 0 && DateTimeOffset.UtcNow - lastFlush >= FlushInterval)
                    {
                        await FlushAsync(buffer, stoppingToken);
                        buffer.Clear();
                        lastFlush = DateTimeOffset.UtcNow;
                    }
                }
            }
            catch (OperationCanceledException) { }
            finally
            {
                if (buffer.Count > 0)
                {
                    try { await FlushAsync(buffer, stoppingToken); }
                    catch (Exception ex) { _logger.LogError(ex, "Final flush failed"); }
                }
                _consumer.Close();
            }
        }

        private async Task FlushAsync(List<ConsumeResult<Ignore, string>> buffer, CancellationToken ct)
        {
            if (buffer.Count == 0) return;

            // 1. Get codes from events
            var codes = buffer
                .Select(r => JsonSerializer.Deserialize<LinkClicked>(r.Message.Value)?.Code)
                .Where(c => !string.IsNullOrEmpty(c))
                .Cast<string>()
                .ToList();

            // 2. One scope per batch
            using var scope = _scopeFactory.CreateScope();
            var cache = scope.ServiceProvider.GetRequiredService<ILinkCache>();
            var repo = scope.ServiceProvider.GetRequiredService<ICodeRepository>();

            // 3. Read and reset counters from Redis
            var counts = await cache.GetAndResetClickCountsAsync(codes, ct);

            if (counts.Count > 0)
            {
                // 4. Write to Postgres
                await repo.BulkIncrementClickCountsAsync(counts, ct);
                _logger.LogInformation("Flushed {Count} codes, {Total} clicks",
                    counts.Count, counts.Values.Sum());
            }

            // 5. Commit offsets for the last message in each partition to avoid reprocessing
            var lastByPartition = buffer
                .GroupBy(r => r.Partition)
                .Select(g => g.Last().TopicPartitionOffset)
                .ToList();

            _consumer.Commit(lastByPartition);
        }


        public override void Dispose()
        {
            _consumer?.Dispose();
            base.Dispose();
        }
    }
}