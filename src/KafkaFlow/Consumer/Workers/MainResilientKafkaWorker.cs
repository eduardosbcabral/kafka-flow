using Confluent.Kafka;
using KafkaFlow.Consumer.Factories;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Polly;
using Polly.CircuitBreaker;

namespace KafkaFlow.Consumer.Workers;

public class MainResilientKafkaWorker<TKey, TValue> : BaseKafkaWorker<TKey, TValue>
{
    private readonly IResilienceBuilderFactory<TKey, TValue> _resilienceBuilderFactory;

    public MainResilientKafkaWorker(
        ILogger<MainResilientKafkaWorker<TKey, TValue>> logger,
        IOptions<ConsumerOptions<TKey, TValue>> consumerOptions,
        IConsumerBuilderFactory<TKey, TValue> consumerBuilderFactory,
        IResilienceBuilderFactory<TKey, TValue> resilienceBuilderFactory,
        IServiceScopeFactory serviceScopeFactory,
        IHostApplicationLifetime applicationLifetime) : base(logger, consumerOptions, consumerBuilderFactory, serviceScopeFactory, applicationLifetime, consumerOptions.Value.Topic)
    {
        _resilienceBuilderFactory = resilienceBuilderFactory;
    }

    protected override async Task<bool> BeforeProcessMessageAsync(CancellationToken cancellationToken)
    {
        var cbPolicy = _resilienceBuilderFactory.GetCircuitBreakerPolicy();
        if (cbPolicy != null && cbPolicy.CircuitState == CircuitState.Open)
        {
            await Task.Delay(5000, cancellationToken);
            return false;
        }

        return true;
    }

    protected override async Task ProcessMessageAsync(IConsumer<TKey, TValue> consumer, IKafkaHandler<TKey, TValue> handler, ConsumeResult<TKey, TValue> result, CancellationToken cancellationToken)
    {
        var consumeContext = new ConsumeContext<TKey, TValue>(result.Message);

        if (await handler.BeforeHandleAsync(consumeContext, cancellationToken).ConfigureAwait(false) == false)
        {
            _logger.LogInformation("Message ignored.");
            return;
        }

        var context = new Context
        {
            ["consumeContext"] = consumeContext
        };

        await _resilienceBuilderFactory.GetAsyncPolicy().ExecuteAsync(async (ctx, ct) =>
        {
            await handler.HandleAsync(consumeContext, cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Message processed successfully.");
        }, context, cancellationToken).ConfigureAwait(false);
    }
}