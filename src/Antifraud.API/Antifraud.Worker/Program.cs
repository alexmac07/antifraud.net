using Antifraud.Service.Extensions;
using Antifraud.Service.KConsumers;

namespace Antifraud.Worker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddServices(builder.Configuration);
 
            builder.Services.AddHostedService<KafkaAntiFraudConsumer>();
 
            var host = builder.Build();
            host.Run();
        }
    }
}