using Antifraud.Common.Settings;
using Antifraud.Model;
using Antifraud.Repository.Implementation;
using Antifraud.Repository.Interface;
using Antifraud.Service.Implementation;
using Antifraud.Service.Interface;
using Antifraud.Service.KConsumers;
using Antifraud.Service.KProducers;
using Antifraud.Service.Validator;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Antifraud.Service.Extensions;

public static class ServiceExtensions
{
    public static void AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        // .: Settings and Configurations
        services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
        services.Configure<KafkaSettings>(configuration.GetSection("KafkaSettings"));
        services.AddHostedService<KafkaConsumer>();


        // .: Utilities :.
        services.AddSingleton<IKafkaProducer, KafkaProducer>();
        services.AddSingleton<IValidator<TransactionModel>, TransactionValidator>();

        // .: Repository Layer :.
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<ITransactionEventRepository, TransactionEventRepository>();

        // .: Service Layer :.
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IAntifraudService, AntifraudService>();


    }
}
