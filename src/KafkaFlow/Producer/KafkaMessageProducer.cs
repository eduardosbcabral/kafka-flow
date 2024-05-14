using Confluent.Kafka;

using KafkaFlow.Consumer;

using Microsoft.Extensions.Logging;

namespace KafkaFlow.Producer;

public class KafkaMessageProducer<TKey, TValue> : IMessageProducer<TKey, TValue>
{
    private readonly ILogger<KafkaMessageProducer<TKey, TValue>> _logger;
    private readonly IProducer<TKey, TValue> _producer;

    public KafkaMessageProducer(
        ILogger<KafkaMessageProducer<TKey, TValue>> logger,
        IProducerBuilderFactory<TKey, TValue> kafkaProducerBuilder)
    {
        _logger = logger;
        _producer = kafkaProducerBuilder.Build();
    }

    public async Task ProduceAsync(string topic, ConsumeContext<TKey, TValue> consumeContext, CancellationToken cancellationToken)
    {
        var message = new Message<TKey, TValue>
        {
            Headers = consumeContext.Headers,
            Key = consumeContext.Key,
            Value = consumeContext.Value
        };

        await _producer.ProduceAsync(topic, message, cancellationToken);

        _logger.LogInformation("Message published to {topic}", topic);
    }
}