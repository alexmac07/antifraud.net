using Antifraud.Common.Settings;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Antifraud.Service.KProducers
{
    public interface IKafkaProducer
    {
        Task PublishAsync<T>(string topic, T sObject);
    }

    public class KafkaProducer : IKafkaProducer
    {
        private readonly IProducer<string, string> _producer;

        public KafkaProducer(IOptions<AppSettings> settings)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = settings.Value.BootstrapServers,
            };

            _producer = new ProducerBuilder<string, string>(config).Build();
        }

        public async Task PublishAsync<T>(string topic, T sObject)
        {
            var key = Guid.NewGuid().ToString();

            var kafkaMessage = new Message<string, string>
            {
                Key = key,
                Value = JsonConvert.SerializeObject(sObject)
            };

            await _producer.ProduceAsync(topic, kafkaMessage);
        }
    }

}
