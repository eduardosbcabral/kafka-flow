using Confluent.Kafka;
using KafkaFlow.Consumer.Factories;
using KafkaFlow.Options;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KafkaFlow.Consumer.Workers;

public class KafkaWorker<TKey, TValue> : BaseKafkaWorker<TKey, TValue>
{
    public KafkaWorker(
        ILogger<KafkaWorker<TKey, TValue>> logger,
        IOptions<ConsumerOptions<TKey, TValue>> consumerOptions,
        IConsumerBuilderFactory<TKey, TValue> consumerBuilderFactory,
        IServiceScopeFactory serviceScopeFactory,
        IHostApplicationLifetime applicationLifetime) : base(logger, consumerOptions, consumerBuilderFactory, serviceScopeFactory, applicationLifetime, consumerOptions.Value.Topic)
    {
    }

    protected override Task<bool> BeforeProcessMessageAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }

    protected override async Task ProcessMessageAsync(IConsumer<TKey, TValue> consumer, IKafkaHandler<TKey, TValue> handler, ConsumeResult<TKey, TValue> result, CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new(result.Message), cancellationToken).ConfigureAwait(false);
    }
}