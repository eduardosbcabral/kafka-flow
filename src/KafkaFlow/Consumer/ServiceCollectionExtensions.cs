using KafkaFlow.Consumer.Factories;
using KafkaFlow.Consumer.Workers;
using KafkaFlow.Options;
using KafkaFlow.Producer;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System.Text.Json;

namespace KafkaFlow.Consumer;

public static class ServiceCollectionConsumerExtensions
{
    public static IServiceCollection ConfigureKafkaWorker<TKey, TValue, THandler>(
        this IServiceCollection services,
        IConfigurationSection section,
        Action<ConsumerOptions<TKey, TValue>>? customConsumerConfig = null,
        JsonSerializerOptions? jsonSerializerOptions = null)
        where THandler : class, IKafkaHandler<TKey, TValue>
    {
        services.Configure<ConsumerOptions<TKey, TValue>>(section);

        if (customConsumerConfig is not null)
        {
            services.PostConfigure(customConsumerConfig);
        }

        services.AddSingleton<IConsumerBuilderFactory<TKey, TValue>, ConsumerBuilderFactory<TKey, TValue>>();
        services.AddSingleton(x => new KafkaDeserializer<TValue>(jsonSerializerOptions));

        services.AddTransient<IKafkaHandler<TKey, TValue>, THandler>();

        _ = bool.TryParse(section["Resilience:Enabled"], out var resilienceEnabled);

        if (resilienceEnabled)
        {
            services.AddSingleton<IResilienceBuilderFactory<TKey, TValue>, ResilienceBuilderFactory<TKey, TValue>>();
            services.AddKafkaProducer<TKey, TValue>(section, jsonSerializerOptions);
            services.AddHostedService<MainResilientKafkaWorker<TKey, TValue>>();
            services.AddHostedService<RetryResilientKafkaWorker<TKey, TValue>>();
        }
        else
        {
            services.AddHostedService<KafkaWorker<TKey, TValue>>();
        }

        return services;
    }
}