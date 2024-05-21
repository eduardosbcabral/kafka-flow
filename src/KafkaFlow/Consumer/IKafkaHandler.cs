namespace KafkaFlow.Consumer;

public abstract class KafkaHandler<TKey, TValue> : IKafkaHandler<TKey, TValue>
{
    public virtual Task<bool> BeforeHandleAsync(ConsumeContext<TKey, TValue> consumeContext, CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }

    public abstract Task HandleAsync(ConsumeContext<TKey, TValue> consumeContext, CancellationToken cancellationToken);
}


public interface IKafkaHandler<TKey, TValue>
{
    Task<bool> BeforeHandleAsync(ConsumeContext<TKey, TValue> consumeContext, CancellationToken cancellationToken);
    Task HandleAsync(ConsumeContext<TKey, TValue> consumeContext, CancellationToken cancellationToken);
}
