using KafkaFlow.Http;
using KafkaFlow.Http.Options;
using KafkaFlow.Options;
using KafkaFlow.Sample.Handlers;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System.Text.Json;

namespace KafkaFlow.Consumer;

public static class ServiceCollectionConsumerExtensions
{
    public static IHttpClientBuilder ConfigureDefaultHttpKafkaWorker<TKey, TValue>(
        this IServiceCollection services,
        IConfigurationSection section,
        Action<ConsumerOptions<TKey, TValue>>? customConsumerConfig = null,
        JsonSerializerOptions? jsonSerializerOptions = null)
    {
        services.Configure<HttpConsumerOptions<TKey, TValue, HttpOptions>>(section);
        services.ConfigureKafkaWorker<TKey, TValue, HttpHandler<TKey, TValue>>(section, customConsumerConfig, jsonSerializerOptions);
        return services.AddHttpClient<HttpService<TKey, TValue>>();
    }

    public static IHttpClientBuilder ConfigureCustomHttpKafkaWorker<TKey, TValue, THttpOptions>(
        this IServiceCollection services,
        IConfigurationSection section,
        Action<ConsumerOptions<TKey, TValue>>? customConsumerConfig = null,
        JsonSerializerOptions? jsonSerializerOptions = null)
        where THttpOptions : HttpOptions
    {
        services.Configure<HttpConsumerOptions<TKey, TValue, THttpOptions>>(section);
        return ConfigureDefaultHttpKafkaWorker(services, section, customConsumerConfig, jsonSerializerOptions);
    }
}