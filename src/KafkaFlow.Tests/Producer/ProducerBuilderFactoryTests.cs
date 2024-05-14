using KafkaFlow.Producer;

using Microsoft.Extensions.Options;

namespace KafkaFlow.Tests.Producer;

public class ProducerBuilderFactoryTests
{
    [Fact]
    public void ConstructorShouldThrowExceptionIfProducerOptionsIsNull()
    {
        IOptions<ProducerOptions<string, string>> producerOptions = null;
        KafkaSerializer<string> kafkaSerializer = new();
        Assert.Throws<ArgumentNullException>(() => new ProducerBuilderFactory<string, string>(producerOptions, kafkaSerializer));
    }


    [Fact]
    public void ConstructorShouldThrowExceptionIfKafkaSerializerIsNull()
    {
        var producerOptions = Options.Create(new ProducerOptions<string, string>());
        KafkaSerializer<string> kafkaSerializer = null;
        Assert.Throws<ArgumentNullException>(() => new ProducerBuilderFactory<string, string>(producerOptions, kafkaSerializer));
    }

    [Fact]
    public void ConstructorShouldCreateProducerBuilderFactory()
    {
        var producerOptions = Options.Create(new ProducerOptions<string, string>());
        var kafkaSerializer = new KafkaSerializer<string>();
        var sut = new ProducerBuilderFactory<string, string>(producerOptions, kafkaSerializer);
        Assert.IsType<ProducerBuilderFactory<string, string>>(sut);
    }

    [Fact]
    public void BuildShouldReturnProducerBuilder()
    {
        var kafkaSerializer = new KafkaSerializer<string>();
        var producerOptions = Options.Create(new ProducerOptions<string, string>());
        var sut = new ProducerBuilderFactory<string, string>(producerOptions, kafkaSerializer);
        var producer = sut.Build();
        Assert.NotNull(producer);
    }
}
