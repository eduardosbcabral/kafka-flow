using KafkaFlow.Producer;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Polly;
using Polly.CircuitBreaker;
using Polly.Contrib.WaitAndRetry;
using Polly.Wrap;

namespace KafkaFlow.Consumer.Factories;

public class ResilienceBuilderFactory<TKey, TValue> : IResilienceBuilderFactory<TKey, TValue>
{
    private readonly IAsyncPolicy _asyncPolicy;
    private readonly AsyncCircuitBreakerPolicy? _circuitBreakerPolicy;

    public ResilienceBuilderFactory(
        ILogger<ResilienceBuilderFactory<TKey, TValue>> logger,
        IOptions<ConsumerOptions<TKey, TValue>> consumerOptions,
        IServiceScopeFactory serviceScopeFactory)
    {
        var consumer = consumerOptions.Value;
        var resilience = consumer.Resilience;
        var retry = resilience.Retry;
        var circuitBreaker = resilience.CircuitBreaker;

        var fallbackPolicy = Policy
            .Handle<Exception>()
            .FallbackAsync(async (context, cancellationToken) =>
            {
                var consumeContext = (ConsumeContext<TKey, TValue>)context["consumeContext"];

                using var scope = serviceScopeFactory.CreateScope();
                var messageProducer = scope.ServiceProvider.GetService<IMessageProducer<TKey, TValue>>();
                
                if (retry.ShouldPublishDirectlyToDeadLetter(consumer.DeadLetter))
                {
                    await messageProducer.ProduceAsync(consumer.DeadLetter, consumeContext, cancellationToken);
                    return;
                }

                if (retry.ShouldPublishToRetry())
                {
                    var retryCount = consumeContext.GetRetryAttempt();
                    if (retryCount >= resilience.Retry.PublishRetryCount)
                    {
                        consumeContext.AddRetryAttempt(retryCount);
                        await messageProducer.ProduceAsync(consumer.DeadLetter, consumeContext, cancellationToken);
                    }
                    else
                    {
                        consumeContext.AddRetryAttempt(retryCount + 1);
                        await messageProducer.ProduceAsync(consumer.Retry, consumeContext, cancellationToken);
                    }
                }
            }, (exception, context) =>
            {
                var consumeContext = (ConsumeContext<TKey, TValue>)context["consumeContext"];
                if (!resilience.Retry.ShouldPublishToRetry())
                {
                    logger.LogWarning("Error when processing a message: {Message}", exception.Message);
                    return Task.CompletedTask;
                }

                var attempt = consumeContext.GetRetryAttempt();
                logger.LogWarning("Error when processing a message: {Message}. Attempt: {attempt}", exception.Message, attempt);
                return Task.CompletedTask;
            });

        AsyncPolicyWrap? asyncPolicies = null;

        if (retry.Enabled && retry.MaxRetryAttempts > 0)
        {
            var jitter = Backoff.DecorrelatedJitterBackoffV2(
                medianFirstRetryDelay: TimeSpan.FromMilliseconds(retry.MedianFirstRetryDelayInMilliseconds),
                retryCount: retry.MaxRetryAttempts);

            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(jitter, onRetry: (exception, timespan, attempt, context) =>
                {
                    logger.LogWarning("Retrying due to: {Message}. Attempt: {attempt}", exception.Message, attempt);
                });

            asyncPolicies = fallbackPolicy.WrapAsync(retryPolicy);

            if (circuitBreaker.Enabled)
            {
                var circuitBreakerPolicy = Policy
                    .Handle<Exception>()
                    .CircuitBreakerAsync(
                        exceptionsAllowedBeforeBreaking: circuitBreaker.ExceptionsAllowedBeforeBreaking,
                        durationOfBreak: TimeSpan.FromSeconds(circuitBreaker.DurationOfBreakInSeconds),
                        onBreak: (ex, breakDelay) =>
                        {
                            logger.LogWarning("Circuit breaker opened for {TotalSeconds} seconds due to: {Message}", breakDelay.TotalSeconds, ex.Message);
                        },
                        onReset: () => logger.LogInformation("Circuit breaker reset."),
                        onHalfOpen: () => logger.LogInformation("Circuit breaker is half-open."));

                asyncPolicies = asyncPolicies.WrapAsync(circuitBreakerPolicy);
                _circuitBreakerPolicy = circuitBreakerPolicy;
            }
        }

        _asyncPolicy = asyncPolicies != null ? asyncPolicies : fallbackPolicy;
    }

    public IAsyncPolicy GetAsyncPolicy()
        => _asyncPolicy;

    public AsyncCircuitBreakerPolicy? GetCircuitBreakerPolicy()
       => _circuitBreakerPolicy;
}

public interface IResilienceBuilderFactory<TKey, TValue>
{
    public IAsyncPolicy GetAsyncPolicy();
    public AsyncCircuitBreakerPolicy? GetCircuitBreakerPolicy();
}
