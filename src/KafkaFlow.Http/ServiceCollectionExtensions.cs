using KafkaFlow.Consumer.Factories;
using KafkaFlow.Consumer.Workers;
using KafkaFlow.Http;
using KafkaFlow.Http.Options;
using KafkaFlow.Options;
using KafkaFlow.Producer;
using KafkaFlow.Sample.Handlers;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using System.Text.Json;

namespace KafkaFlow.Consumer;

public static class ServiceCollectionConsumerExtensions
{
    public static IServiceCollection ConfigureDefaultHttpKafkaWorker<TKey, TValue>(
        this IServiceCollection services,
        IConfigurationSection section,
        Action<ConsumerOptions<TKey, TValue>>? customConsumerConfig = null,
        JsonSerializerOptions? jsonSerializerOptions = null)
    {
        services.Configure<HttpConsumerOptions<TKey, TValue>>(section);

        services.AddSingleton(x =>
        {
            var options = x.GetRequiredService<IOptions<HttpConsumerOptions<TKey, TValue>>>();
            return new JwtAuthenticationHandler<TKey, TValue>(options, jsonSerializerOptions);
        });
        services.AddHttpClient<HttpService>().AddHttpMessageHandler<JwtAuthenticationHandler<TKey, TValue>>();

        return services.ConfigureKafkaWorker<TKey, TValue, HttpHandler<TKey, TValue>>(section, customConsumerConfig, jsonSerializerOptions);
    }
}