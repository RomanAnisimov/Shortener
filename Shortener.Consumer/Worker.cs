using Confluent.Kafka;
using Shortener.Application.Abstractions.Interfaces;
using Shortener.Shared;

namespace Shortener.Consumer
{
    public class Worker : BackgroundService
    {
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
            _consumer.Subscribe(new[] { KafkaTopics.LinkCreated, KafkaTopics.LinkClicked });
            _logger.LogInformation("Consumer started");

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var result = _consumer.Consume(stoppingToken);

                    try
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var handler = scope.ServiceProvider.GetRequiredService<IKafkaConsumerService>();

                        await handler.HandleAsync(result, stoppingToken);
                        _consumer.Commit(result);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Failed to handle message from {Topic} at {Offset}",
                            result.Topic, result.Offset);
                    }
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

        public override void Dispose()
        {
            _consumer?.Dispose();
            base.Dispose();
        }
    }
}