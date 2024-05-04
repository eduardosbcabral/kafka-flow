using KafkaFlow.Consumer;

namespace KafkaFlow.Producer;

public interface IMessageProducer<TKey, TValue>
{
    Task ProduceAsync(string topic, ConsumeContext<TKey, TValue> consumeContext, CancellationToken cancellationToken);
}