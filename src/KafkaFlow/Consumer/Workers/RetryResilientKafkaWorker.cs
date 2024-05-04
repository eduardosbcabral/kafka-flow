using Confluent.Kafka;

using KafkaFlow.Consumer.Factories;
using KafkaFlow.Options;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Polly;
using Polly.CircuitBreaker;

namespace KafkaFlow.Consumer.Workers;

public class RetryResilientKafkaWorker<TKey, TValue> : BaseKafkaWorker<TKey, TValue>
{
    private readonly IResilienceBuilderFactory<TKey, TValue> _resilienceBuilderFactory;

    public RetryResilientKafkaWorker(
        ILogger<MainResilientKafkaWorker<TKey, TValue>> logger,
        IOptions<ConsumerOptions<TKey, TValue>> consumerOptions,
        IConsumerBuilderFactory<TKey, TValue> consumerBuilderFactory,
        IResilienceBuilderFactory<TKey, TValue> resilienceBuilderFactory,
        IServiceScopeFactory serviceScopeFactory,
        IHostApplicationLifetime applicationLifetime) : base(logger, consumerOptions, consumerBuilderFactory, serviceScopeFactory, applicationLifetime, consumerOptions.Value.Retry)
    {
        _resilienceBuilderFactory = resilienceBuilderFactory;
    }

    protected override async Task<bool> BeforeProcessMessageAsync(CancellationToken cancellationToken)
    {
        if (_resilienceBuilderFactory.GetCircuitBreakerPolicy().CircuitState == CircuitState.Open)
        {
            await Task.Delay(5000, cancellationToken);
            return false;
        }

        return true;
    }

    protected override async Task ProcessMessageAsync(IConsumer<TKey, TValue> consumer, IKafkaHandler<TKey, TValue> handler, ConsumeResult<TKey, TValue> result, CancellationToken cancellationToken)
    {
        var consumeContext = new ConsumeContext<TKey, TValue>(result.Message);

        var context = new Context
        {
            ["consumeContext"] = consumeContext
        };

        await _resilienceBuilderFactory.GetAsyncPolicy().ExecuteAsync(async (ctx, ct) =>
        {
            await handler.HandleAsync(consumeContext, cancellationToken).ConfigureAwait(false);
        }, context, cancellationToken).ConfigureAwait(false);
    }
}