using Confluent.Kafka;

using KafkaFlow.Consumer;
using KafkaFlow.Consumer.Factories;

using Microsoft.Extensions.Options;

namespace KafkaFlow.Tests.Consumer.Factories;

public class ConsumerBuilderFactoryTests
{
    [Fact]
    public void ConstructorShouldThrowExceptionIfConsumerOptionsIsNull()
    {
        IOptions<ConsumerOptions<string, string>> consumerOptions = null;
        KafkaDeserializer<string> kafkaDeserializer = new();
        Assert.Throws<ArgumentNullException>(() => new ConsumerBuilderFactory<string, string>(consumerOptions, kafkaDeserializer));
    }


    [Fact]
    public void ConstructorShouldThrowExceptionIfKafkaDeserializerIsNull()
    {
        var consumerOptions = Options.Create(new ConsumerOptions<string, string>());
        KafkaDeserializer<string> kafkaDeserializer = null;
        Assert.Throws<ArgumentNullException>(() => new ConsumerBuilderFactory<string, string>(consumerOptions, kafkaDeserializer));
    }

    [Fact]
    public void ConstructorShouldCreateConsumerBuilderFactory()
    {
        var consumerOptions = Options.Create(new ConsumerOptions<string, string>());
        var kafkaDeserializer = new KafkaDeserializer<string>();
        var sut = new ConsumerBuilderFactory<string, string>(consumerOptions, kafkaDeserializer);
        Assert.IsType<ConsumerBuilderFactory<string, string>>(sut);
    }

    [Fact]
    public void BuildShouldReturnConsumerBuilder()
    {
        var kafkaDeserializer = new KafkaDeserializer<string>();

        var consumerOptions = Options.Create(new ConsumerOptions<string, string>());

        var sut = new ConsumerBuilderFactory<string, string>(consumerOptions, kafkaDeserializer);

        var consumer = sut.Build();

        Assert.NotNull(consumer);
    }
}
