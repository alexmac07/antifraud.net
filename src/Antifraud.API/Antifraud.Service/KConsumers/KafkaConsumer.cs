using Antifraud.Common.Settings;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Antifraud.Service.KConsumers
{
    public class KafkaConsumer : BackgroundService
    {
        private readonly AppSettings _appSettings;
        private readonly IConsumer<string, string> _consumer;

        public KafkaConsumer(IOptions<AppSettings> appSettings, IConsumer<string, string> consumer)
        {
            _appSettings = appSettings.Value ?? throw new ArgumentNullException(nameof(appSettings));
            _consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));

            var config = new ConsumerConfig
            {
                BootstrapServers = _appSettings.BootstrapServers,
                GroupId = "transaction-consumer-group",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            _consumer = new ConsumerBuilder<string, string>(config).Build();
            _consumer.Subscribe("transactions");

        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }
    }
}
