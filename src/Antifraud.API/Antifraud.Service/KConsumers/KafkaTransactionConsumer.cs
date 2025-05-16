using Antifraud.Common.Globalization;
using Antifraud.Common.Settings;
using Antifraud.Dto;
using Antifraud.Model;
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
    public class KafkaTransactionConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly AppSettings _appSettings;
        private readonly KafkaSettings _kafkaSettings;
        private readonly ILogger<KafkaTransactionConsumer> _logger;

        public KafkaTransactionConsumer(IServiceScopeFactory scopeFactory,
                             IOptions<AppSettings> appSettings,
                             IOptions<KafkaSettings> kafkaSettings,
                             ILogger<KafkaTransactionConsumer> logger)
        {
            _scopeFactory = scopeFactory;
            _appSettings = appSettings.Value ?? throw new ArgumentNullException(nameof(appSettings));
            _kafkaSettings = kafkaSettings.Value ?? throw new ArgumentException(nameof(kafkaSettings));
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = _kafkaSettings.BootstrapServers,
                GroupId = _kafkaSettings.GroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using var consumer = new ConsumerBuilder<string, string>(config).Build();
            consumer.Subscribe(_kafkaSettings.ConfirmationTopic);
            _logger.LogInformation($"Suscribed to {_kafkaSettings.ConfirmationTopic}");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = consumer.Consume(stoppingToken);

                    if (result?.Message != null)
                    {
                        _logger.LogInformation($"Received transaction {result.Message.Value}");

                        if (string.IsNullOrEmpty(result.Message.Value))
                            continue;

                        var antiFraudTransactionResponse = JsonConvert.DeserializeObject<TransactionResult>(result.Message.Value);

                        if (antiFraudTransactionResponse == null)
                        {
                            _logger.LogError($"{Languages.RejectionInvalidTransaction} | {result}");
                            continue;
                        }
                        // Process update transaction service
                        using var scope = _scopeFactory.CreateScope();
                        var transactionService = scope.ServiceProvider.GetRequiredService<ITransactionService>();
                        TransactionModel antifraudResult = await transactionService.UpdateTransaction(antiFraudTransactionResponse);
                    }

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

            consumer.Close();
            _logger.LogInformation("Kafka consumer closed");


        }
    }
}
