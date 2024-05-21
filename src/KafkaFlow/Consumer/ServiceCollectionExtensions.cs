using Confluent.Kafka;

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
    public static IKafkaWorkerBuilder<TKey, TValue> ConfigureKafkaWorker<TKey, TValue>(
        this IServiceCollection services,
        IConfigurationSection section)
    {
        return new KafkaWorkerBuilder<TKey, TValue>(services, section);
    }
}

public interface IKafkaWorkerBuilder<TKey, TValue> : IServiceCollection
{
    IServiceCollection Services { get; }
    IConfigurationSection Section { get; }
    JsonSerializerOptions? JsonSerializerOptions { get; }
    IKafkaWorkerBuilder<TKey, TValue> ConfigureConsumerOptions(Action<ConsumerOptions<TKey, TValue>> customConsumerConfig);
    IKafkaWorkerBuilder<TKey, TValue> WithCustomSerialization(JsonSerializerOptions jsonSerializerOptions);
    IKafkaWorkerBuilder<TKey, TValue> WithHandler<THandler>() where THandler : class, IKafkaHandler<TKey, TValue>;
    void Configure();
}

public class KafkaWorkerBuilder<TKey, TValue> : ServiceCollection, IKafkaWorkerBuilder<TKey, TValue>
{
    public IServiceCollection Services { get; protected set; }
    public IConfigurationSection Section { get; protected set; }
    public JsonSerializerOptions? JsonSerializerOptions { get; protected set; }

    private bool _handlerConfigured = false;

    public KafkaWorkerBuilder(IServiceCollection services, IConfigurationSection section)
    {
        Services = services;

        Services
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

        Section = section;

        // Default configurations because the worker does not do auto commit
        Services.PostConfigure<ConsumerOptions<TKey, TValue>>(x =>
        {
            x.EnableAutoCommit = false;
            x.AutoOffsetReset = AutoOffsetReset.Latest;
        });
    }

    public IKafkaWorkerBuilder<TKey, TValue> ConfigureConsumerOptions(Action<ConsumerOptions<TKey, TValue>> customConsumerConfig)
    {
        Services.PostConfigure(customConsumerConfig);
        return this;
    }

    public IKafkaWorkerBuilder<TKey, TValue> WithCustomSerialization(JsonSerializerOptions jsonSerializerOptions)
    {
        JsonSerializerOptions = jsonSerializerOptions;
        return this;
    }

    public IKafkaWorkerBuilder<TKey, TValue> WithHandler<THandler>()
        where THandler : class, IKafkaHandler<TKey, TValue>
    {
        Services.AddTransient<IKafkaHandler<TKey, TValue>, THandler>();
        _handlerConfigured = true;
        return this;
    }

    public void Configure()
    {
        if (!_handlerConfigured)
        {
            throw new ValidationException($"KafkaWorker<{typeof(TKey).Name}><{typeof(TValue).Name}>: Handler not configured.");
        }

        Services.AddSingleton<IConsumerBuilderFactory<TKey, TValue>, ConsumerBuilderFactory<TKey, TValue>>();
        Services.AddSingleton(x => new KafkaDeserializer<TValue>(JsonSerializerOptions));

        var consumerOptions = Section.Get<ConsumerOptions<TKey, TValue>>();
        ArgumentNullException.ThrowIfNull(consumerOptions, nameof(consumerOptions));

        Services.AddSingleton<IResilienceBuilderFactory<TKey, TValue>, ResilienceBuilderFactory<TKey, TValue>>();
        Services.AddHostedService<MainResilientKafkaWorker<TKey, TValue>>();

        if (consumerOptions.Resilience.Retry.ShouldPublishToRetry() || consumerOptions.Resilience.Retry.ShouldPublishDirectlyToDeadLetter(consumerOptions.DeadLetter))
        {
            Services.AddKafkaProducer<TKey, TValue>(Section, JsonSerializerOptions);
        }

        if (consumerOptions.Resilience.Retry.ShouldPublishToRetry())
        {
            Services.AddHostedService<RetryResilientKafkaWorker<TKey, TValue>>();
        }
    }
}