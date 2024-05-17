namespace KafkaFlow.Consumer;

public interface IKafkaHandler<TKey, TValue>
{
    Task<bool> BeforeHandleAsync(ConsumeContext<TKey, TValue> consumeContext, CancellationToken cancellationToken);
    Task HandleAsync(ConsumeContext<TKey, TValue> consumeContext, CancellationToken cancellationToken);
}
