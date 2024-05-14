using KafkaFlow.Consumer.Factories;
using KafkaFlow.Consumer.Workers;
using KafkaFlow.Producer;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System.ComponentModel.DataAnnotations;
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
        services
            .AddOptions<ConsumerOptions<TKey, TValue>>()
            .Bind(section)
            .Validate(x =>
            {
                var isValid = x.Validate(out var errorMessage);
                if (!isValid)
                {
                    throw new ValidationException($"ConsumerOptions<{typeof(TKey).Name}><{typeof(TValue).Name}>: {errorMessage}");
                }

                return true;
            });

        if (customConsumerConfig is not null)
        {
            services.PostConfigure(customConsumerConfig);
        }

        services.AddSingleton<IConsumerBuilderFactory<TKey, TValue>, ConsumerBuilderFactory<TKey, TValue>>();
        services.AddSingleton(x => new KafkaDeserializer<TValue>(jsonSerializerOptions));

        services.AddTransient<IKafkaHandler<TKey, TValue>, THandler>();

        var consumerOptions = section.Get<ConsumerOptions<TKey, TValue>>();

        services.AddSingleton<IResilienceBuilderFactory<TKey, TValue>, ResilienceBuilderFactory<TKey, TValue>>();
        services.AddHostedService<MainResilientKafkaWorker<TKey, TValue>>();

        if (consumerOptions.Resilience.Retry.ShouldPublishToRetry() || consumerOptions.Resilience.Retry.ShouldPublishDirectlyToDeadLetter(consumerOptions.DeadLetter))
        {
            services.AddKafkaProducer<TKey, TValue>(section, jsonSerializerOptions);
        }

        if (consumerOptions.Resilience.Retry.ShouldPublishToRetry())
        {
            services.AddHostedService<RetryResilientKafkaWorker<TKey, TValue>>();
        }

        return services;
    }
}