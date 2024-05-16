using Confluent.Kafka;

using KafkaFlow.Consumer;
using KafkaFlow.Sample.Handlers;
using KafkaFlow.Sample.Http;
using KafkaFlow.Sample.Messages;

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
        services.ConfigureKafkaWorker<string, KafkaMessage<SampleMessage>>(consumer1Config)
            .ConfigureConsumerOptions(x =>
            {
                x.EnableAutoCommit = false;
                x.AutoOffsetReset = AutoOffsetReset.Latest;
            })
            .WithHandler<SampleMessageHandler>()
            .Configure();

        var consumer2Config = host.Configuration.GetRequiredSection("Consumer2");
        services.ConfigureHttpKafkaWorker<string, AnotherSampleMessage>(consumer2Config)
            .ConfigureConsumerOptions(x =>
            {
                x.EnableAutoCommit = false;
                x.AutoOffsetReset = AutoOffsetReset.Latest;
            })
            .WithCustomSerialization(new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            })
            .WithHandler<AnotherSampleMessageHandler>()
            .Configure();

        // Jwt Authentication Example
        var consumer3Config = host.Configuration.GetRequiredSection("Consumer3");
        services.ConfigureHttpKafkaWorker<string, OtherSampleMessage>(consumer3Config)
            .ConfigureConsumerOptions(x =>
            {
                x.EnableAutoCommit = false;
                x.AutoOffsetReset = AutoOffsetReset.Latest;
            })
            .WithCustomHttpOptions<JwtHttpOptions>()
            .WithHttpMessageHandler<JwtAuthenticationHandler<string, OtherSampleMessage>>()
            .WithDefaultHttpHandler()
            .Configure();
    });

var host = builder.Build();
host.Run();
