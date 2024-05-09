using Confluent.Kafka;

using KafkaFlow;
using KafkaFlow.Consumer;
using KafkaFlow.Sample.Handlers;
using KafkaFlow.Sample.Messages;

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

        //var consumer2Config = host.Configuration.GetRequiredSection("Consumer2");
        //services.ConfigureDefaultHttpKafkaWorker<string, AnotherSampleMessage>(consumer2Config, x =>
        //{
        //    x.EnableAutoCommit = false;
        //    x.AutoOffsetReset = AutoOffsetReset.Latest;
        //}, new System.Text.Json.JsonSerializerOptions()
        //{
        //    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        //});
    });

var host = builder.Build();
host.Run();
