namespace KafkaFlow.Consumer;

public interface IKafkaHandler<TKey, TValue>
{
    Task HandleAsync(ConsumeContext<TKey, TValue> consumeContext, CancellationToken cancellationToken);
}
