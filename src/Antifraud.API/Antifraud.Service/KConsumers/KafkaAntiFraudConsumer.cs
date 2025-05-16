using Antifraud.Common.Globalization;
using Antifraud.Common.Settings;
using Antifraud.Dto;
using Antifraud.Service.Interface;
using Antifraud.Service.KProducers;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Antifraud.Service.KConsumers
{
    public class KafkaAntiFraudConsumer : BackgroundService
    {
        private readonly IKafkaProducer _kafkaProducer;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly AppSettings _appSettings;
        private readonly KafkaSettings _kafkaSettings;
        private readonly IConsumer<string, string> _consumer;
        private readonly IOptions<KafkaSettings> _kafkaOptions;
        private readonly ILogger<KafkaAntiFraudConsumer> _logger;

        public KafkaAntiFraudConsumer(IKafkaProducer kafkaProducer,
                                      IServiceScopeFactory scopeFactory,
                                      IOptions<AppSettings> appSettings,
                                      IOptions<KafkaSettings> kafkaSettings,
                                      ILogger<KafkaAntiFraudConsumer> logger)
        {
            _kafkaProducer = kafkaProducer ?? throw new ArgumentNullException(nameof(kafkaProducer));
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _kafkaOptions = kafkaSettings ?? throw new ArgumentNullException(nameof(kafkaSettings));
            _appSettings = appSettings.Value ?? throw new ArgumentNullException(nameof(appSettings));
            _kafkaSettings = kafkaSettings.Value ?? throw new ArgumentException(nameof(kafkaSettings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

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
                try
                {
                    var result = _consumer.Consume(stoppingToken);

                    _logger.LogInformation($"Received transaction {result.Message.Value}");

                    if (string.IsNullOrEmpty(result.Message.Value))
                        continue;

                    var transaction = JsonConvert.DeserializeObject<TransactionEventKafkaDto>(result.Message.Value);

                    if (transaction == null)
                    {
                        _logger.LogError($"{Languages.RejectionInvalidTransaction} | {result}");
                        throw new ArgumentException(Languages.RejectionInvalidTransaction);
                    }
                    // Process antifraud service
                    using var scope = _scopeFactory.CreateScope();
                    var antifraudService = scope.ServiceProvider.GetRequiredService<IAntifraudService>();
                    TransactionResult antifraudResult = await antifraudService.VerifyTransaction(transaction.EventId, transaction.TransactionId);

                    
                    await _kafkaProducer.PublishAsync(_kafkaSettings.ConfirmationTopic, antifraudResult);
                }
                catch (OperationCanceledException oce)
                {
                    _logger.LogWarning("Operation cancelled");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled exception occurred");

                }
            }

            _consumer.Close();

        }
    }
}
