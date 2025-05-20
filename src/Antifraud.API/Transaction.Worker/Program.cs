using Antifraud.Service.Extensions;
using Antifraud.Service.Implementation;
using Antifraud.Service.Interface;
using Antifraud.Service.KConsumers;

namespace Transaction.Worker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddServices(builder.Configuration);
            builder.Services.AddScoped<ITransactionService, TransactionService>();
            builder.Services.AddHostedService<KafkaTransactionConsumer>();

            var host = builder.Build();
            host.Run();
        }
    }
}