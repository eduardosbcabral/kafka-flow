using Confluent.Kafka;
using KafkaFlow.Consumer;
using KafkaFlow.Producer;

using Microsoft.Extensions.Logging;

using Moq;

namespace KafkaFlow.Tests.Producer;

public class KafkaMessageProducerTests
{
    [Fact]
    public async Task ProduceAsync_PublishesMessageWithCorrectData()
    {
        // Arrange
        var topic = "test-topic";
        var key = "test-key";
        var value = "test-value";
        var consumeContext = new ConsumeContext<string, string>(key, value, new Headers());
        consumeContext.AddRetryAttempt(1);
        var cancellationToken = CancellationToken.None;

        var loggerMock = new Mock<ILogger<KafkaMessageProducer<string, string>>>();

        var producerMock = new Mock<IProducer<string, string>>();
        var producerFactoryMock = new Mock<IProducerBuilderFactory<string, string>>();
        producerFactoryMock.Setup(x => x.Build()).Returns(producerMock.Object);

        var producer = new KafkaMessageProducer<string, string>(loggerMock.Object, producerFactoryMock.Object);

        // Act
        await producer.ProduceAsync(topic, consumeContext, cancellationToken);

        // Assert
        producerMock.Verify(x => x.ProduceAsync(topic,
            It.Is<Message<string, string>>(m => m.Key == consumeContext.Key && m.Value == consumeContext.Value && m.Headers.Count == consumeContext.Headers.Count),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
