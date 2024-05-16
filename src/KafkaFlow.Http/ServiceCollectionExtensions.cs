using KafkaFlow.Http;
using KafkaFlow.Http.Options;
using KafkaFlow.Sample.Handlers;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System.Text.Json;

namespace KafkaFlow.Consumer;

public static class ServiceCollectionConsumerExtensions
{
    public static IHttpKafkaWorkerBuilder<TKey, TValue> ConfigureHttpKafkaWorker<TKey, TValue>(
        this IServiceCollection services,
        IConfigurationSection section)
    {
        var kafkaWorkerBuilder = services.ConfigureKafkaWorker<TKey, TValue>(section);
        return new HttpKafkaWorkerBuilder<TKey, TValue>(kafkaWorkerBuilder);
    }
}

public interface IHttpKafkaWorkerBuilder<TKey, TValue> : IKafkaWorkerBuilder<TKey, TValue>
{
    IHttpClientBuilder HttpClientBuilder { get; }

    IHttpKafkaWorkerBuilder<TKey, TValue> WithHttpMessageHandler<THttpMessageHandler>(ServiceLifetime serviceLifetime = ServiceLifetime.Singleton) where THttpMessageHandler : DelegatingHandler;
    IHttpKafkaWorkerBuilder<TKey, TValue> WithDefaultHttpHandler();
    IHttpKafkaWorkerBuilder<TKey, TValue> WithCustomHttpOptions<THttpOptions>() where THttpOptions : HttpOptions;

    new IHttpKafkaWorkerBuilder<TKey, TValue> ConfigureConsumerOptions(Action<ConsumerOptions<TKey, TValue>> customConsumerConfig);
    new IHttpKafkaWorkerBuilder<TKey, TValue> WithCustomSerialization(JsonSerializerOptions jsonSerializerOptions);
    new IHttpKafkaWorkerBuilder<TKey, TValue> WithHandler<THandler>() where THandler : class, IKafkaHandler<TKey, TValue>;
}

public class HttpKafkaWorkerBuilder<TKey, TValue> : KafkaWorkerBuilder<TKey, TValue>, IHttpKafkaWorkerBuilder<TKey, TValue>
{
    public IHttpClientBuilder HttpClientBuilder { get; private set; }

    public HttpKafkaWorkerBuilder(IKafkaWorkerBuilder<TKey, TValue> kafkaWorkerBuilder) : base(kafkaWorkerBuilder.Services, kafkaWorkerBuilder.Section)
    {
        JsonSerializerOptions = kafkaWorkerBuilder.JsonSerializerOptions;
        Services.Configure<HttpConsumerOptions<TKey, TValue, HttpOptions>>(Section);
        HttpClientBuilder = Services.AddHttpClient<HttpService<TKey, TValue>>();
    }

    public IHttpKafkaWorkerBuilder<TKey, TValue> WithHttpMessageHandler<THttpMessageHandler>(ServiceLifetime serviceLifetime = ServiceLifetime.Singleton)
        where THttpMessageHandler : DelegatingHandler
    {
        Services.Add(new ServiceDescriptor(typeof(THttpMessageHandler), typeof(THttpMessageHandler), serviceLifetime));
        HttpClientBuilder.AddHttpMessageHandler<THttpMessageHandler>();
        return this;
    }

    public IHttpKafkaWorkerBuilder<TKey, TValue> WithDefaultHttpHandler()
    {
        Services.AddTransient<HttpHandler<TKey, TValue>>();
        return this;
    }

    public IHttpKafkaWorkerBuilder<TKey, TValue> WithCustomHttpOptions<THttpOptions>() where THttpOptions : HttpOptions
    {
        Services.Configure<HttpConsumerOptions<TKey, TValue, THttpOptions>>(Section);
        return this;
    }

    IHttpKafkaWorkerBuilder<TKey, TValue> IHttpKafkaWorkerBuilder<TKey, TValue>.ConfigureConsumerOptions(Action<ConsumerOptions<TKey, TValue>> customConsumerConfig)
    {
        ConfigureConsumerOptions(customConsumerConfig);
        return this;
    }

    IHttpKafkaWorkerBuilder<TKey, TValue> IHttpKafkaWorkerBuilder<TKey, TValue>.WithCustomSerialization(JsonSerializerOptions jsonSerializerOptions)
    {
        WithCustomSerialization(jsonSerializerOptions);
        return this;
    }

    IHttpKafkaWorkerBuilder<TKey, TValue> IHttpKafkaWorkerBuilder<TKey, TValue>.WithHandler<THandler>()
    {
        WithHandler<THandler>();
        return this;
    }
}