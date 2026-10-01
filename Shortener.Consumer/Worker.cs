using Confluent.Kafka;
using Shortener.Application.Abstractions.Interfaces;
using Shortener.Shared;

namespace Shortener.Consumer
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IKafkaConsumerService _kafkaConsumerService;
        private readonly IConsumer<Ignore, string> _consumer;


        public Worker(ILogger<Worker> logger,
            IConfiguration config,
            IKafkaConsumerService kafkaConsumerService)
        {
            _logger = logger;
            _kafkaConsumerService = kafkaConsumerService;

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
                    await _kafkaConsumerService.HandleAsync(result, stoppingToken);
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

        public override void Dispose()
        {
            _consumer?.Dispose();
            base.Dispose();
        }
    }
}