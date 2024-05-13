using KafkaFlow.Options;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using System.Text.Json;

namespace KafkaFlow.Producer;

public static class ServiceCollectionProducerExtensions
{
    public static IServiceCollection AddKafkaProducer<TKey, TValue>(
        this IServiceCollection services,
        IConfigurationSection section,
        JsonSerializerOptions? jsonSerializerOptions = null)
    {
        services
            .AddOptions<ProducerOptions<TKey, TValue>>()
            .Bind(section);

        services.AddSingleton(x => new KafkaSerializer<TValue>(jsonSerializerOptions));
        services.TryAddSingleton<IKafkaProducerBuilderFactory<TKey, TValue>, KafkaProducerBuilderFactory<TKey, TValue>>();
        services.TryAddSingleton<IMessageProducer<TKey, TValue>, KafkaMessageProducer<TKey, TValue>>();

        return services;
    }
}
