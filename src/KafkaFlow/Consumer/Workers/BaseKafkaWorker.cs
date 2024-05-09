using Confluent.Kafka;

using KafkaFlow.Consumer.Factories;
using KafkaFlow.Options;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KafkaFlow.Consumer.Workers;

public abstract class BaseKafkaWorker<TKey, TValue> : BackgroundService
{
    protected readonly ILogger<BaseKafkaWorker<TKey, TValue>> _logger;
    protected readonly ConsumerOptions<TKey, TValue> _consumerOptions;
    protected readonly IConsumerBuilderFactory<TKey, TValue> _consumerBuilderFactory;
    protected readonly IServiceScopeFactory _serviceScopeFactory;
    protected readonly IHostApplicationLifetime _applicationLifetime;

    protected readonly string _topic;

    public BaseKafkaWorker(
        ILogger<BaseKafkaWorker<TKey, TValue>> logger,
        IOptions<ConsumerOptions<TKey, TValue>> consumerOptions,
        IConsumerBuilderFactory<TKey, TValue> consumerBuilderFactory,
        IServiceScopeFactory serviceScopeFactory,
        IHostApplicationLifetime applicationLifetime,
        string topic)
    {
        _logger = logger;

        _consumerOptions = consumerOptions.Value;
        _consumerBuilderFactory = consumerBuilderFactory;

        _serviceScopeFactory = serviceScopeFactory;
        _applicationLifetime = applicationLifetime;

        _topic = topic;

        Console.CancelKeyPress += OnCancelKeyPress!;
    }


    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(async () => await StartConsumer(stoppingToken), stoppingToken);
    }

    private async Task StartConsumer(CancellationToken stoppingToken)
    {
        stoppingToken.Register(() => _logger.LogInformation("Service stopping by cancellation token."));

        using var scope = _serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IKafkaHandler<TKey, TValue>>();
        var consumer = _consumerBuilderFactory.Build(_consumerOptions).Build();

        consumer.Subscribe(_topic);
        _logger.LogInformation("Consumer {WorkerName}<{TKeyName}><{TKeyName}> started on topic {Topic}.", GetType().Name, typeof(TKey).Name, typeof(TValue).Name, _topic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var valid = await BeforeProcessMessageAsync(stoppingToken);
                if (!valid) continue;

                ConsumeResult<TKey, TValue> result = null!;

                try
                {
                    result = consumer.Consume(stoppingToken);
                    if (result.Message != null)
                    {
                        _logger.LogDebug("Processing message from {TopicPartitionOffset}.", result.TopicPartitionOffset);

                        try
                        {
                            await ProcessMessageAsync(consumer, handler, result, stoppingToken).ConfigureAwait(false);
                            consumer.Commit(result);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to process message at {TopicPartitionOffset}.", result.TopicPartitionOffset);
                        }
                    }
                }
                catch (ConsumeException e)
                {
                    _logger.LogError("Error consuming message at {TopicPartitionOffset}: {Reason}", result?.TopicPartitionOffset, e.Error.Reason);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Operation canceled during message consumption.");
                }
            }
        }
        finally
        {
            consumer.Close();
            _logger.LogInformation("Kafka consumer closed.");
        }
    }

    protected abstract Task<bool> BeforeProcessMessageAsync(CancellationToken cancellationToken);

    protected abstract Task ProcessMessageAsync(IConsumer<TKey, TValue> consumer, IKafkaHandler<TKey, TValue> handler, ConsumeResult<TKey, TValue> result, CancellationToken cancellationToken);

    private void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
    {
        _logger.LogInformation("Cancellation requested via console key press.");
        e.Cancel = true;
        _applicationLifetime.StopApplication();
    }
}