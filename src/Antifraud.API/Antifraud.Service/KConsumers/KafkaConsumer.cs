using Antifraud.Common.Globalization;
using Antifraud.Common.Settings;
using Antifraud.Dto;
using Antifraud.Service.Interface;
using Antifraud.Service.KProducers;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Antifraud.Service.KConsumers
{
    public class KafkaConsumer : BackgroundService
    {
        private readonly IAntifraudService _antifraudService;
        private readonly AppSettings _appSettings;
        private readonly KafkaSettings _kafkaSettings;
        private readonly IConsumer<string, string> _consumer;
        private readonly IOptions<KafkaSettings> _kafkaOptions;
        private readonly ILogger<KafkaConsumer> _logger;

        public KafkaConsumer(IAntifraudService antifraudService,
                             IOptions<AppSettings> appSettings,
                             IOptions<KafkaSettings> kafkaSettings,
                             IConsumer<string, string> consumer,
                             ILogger<KafkaConsumer> logger)
        {
            _antifraudService = antifraudService;
            _kafkaOptions = kafkaSettings;
            _appSettings = appSettings.Value ?? throw new ArgumentNullException(nameof(appSettings));
            _kafkaSettings = kafkaSettings.Value ?? throw new ArgumentException(nameof(kafkaSettings));
            _consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
            _logger = logger;

            var config = new ConsumerConfig
            {
                BootstrapServers = _kafkaSettings.BootstrapServers,
                GroupId = _kafkaSettings.GroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            _consumer = new ConsumerBuilder<string, string>(config).Build();
            _consumer.Subscribe(_kafkaSettings.TransactionsTopic);

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var result = _consumer.Consume(stoppingToken);

                _logger.LogInformation($"Received transaction {result}");

                var transaction = JsonConvert.DeserializeObject<TransactionEventKafkaDto>(result.Message.Value);

                if (transaction == null)
                {
                    _logger.LogError($"{Languages.RejectionInvalidTransaction} | {result}");
                    throw new ArgumentException(Languages.RejectionInvalidTransaction);
                }
                // Process antifraud service
                TransactionResult antifraudResult = await _antifraudService.VerifyTransaction(transaction.EventId, transaction.TransactionId);
                
                var producer = new KafkaProducer(_kafkaOptions);
                await producer.PublishAsync(_kafkaSettings.ConfirmationTopic, antifraudResult);
            }
        }
    }
}
