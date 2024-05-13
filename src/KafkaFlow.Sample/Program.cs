using Confluent.Kafka;

using KafkaFlow;
using KafkaFlow.Consumer;
using KafkaFlow.Http.Options;
using KafkaFlow.Sample.Handlers;
using KafkaFlow.Sample.Http;
using KafkaFlow.Sample.Messages;

using Microsoft.Extensions.Options;

using System.Text.Json;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((host, services) =>
    {
        services.Configure<HostOptions>(options =>
        {
            options.ServicesStartConcurrently = true;
            options.ServicesStopConcurrently = true;
        });

        var consumer1Config = host.Configuration.GetRequiredSection("Consumer1");
        services.ConfigureKafkaWorker<string, KafkaMessage<SampleMessage>, SampleMessageHandler>(consumer1Config, x =>
        {
            x.EnableAutoCommit = false;
            x.AutoOffsetReset = AutoOffsetReset.Latest;
        });

        var consumer2Config = host.Configuration.GetRequiredSection("Consumer2");
        services.ConfigureDefaultHttpKafkaWorker<string, AnotherSampleMessage>(consumer2Config, x =>
        {
            x.EnableAutoCommit = false;
            x.AutoOffsetReset = AutoOffsetReset.Latest;
        }, 
        new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        // Jwt Authentication Example
        var consumer3Config = host.Configuration.GetRequiredSection("Consumer3");
        services.AddSingleton<JwtAuthenticationHandler<string, OtherSampleMessage>>();
        services.ConfigureCustomHttpKafkaWorker<string, OtherSampleMessage, JwtHttpOptions>(consumer3Config, x =>
        {
            x.EnableAutoCommit = false;
            x.AutoOffsetReset = AutoOffsetReset.Latest;
        }).AddHttpMessageHandler<JwtAuthenticationHandler<string, OtherSampleMessage>>();
    });

var host = builder.Build();
host.Run();
