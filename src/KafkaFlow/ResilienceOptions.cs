using System.ComponentModel.DataAnnotations;

namespace KafkaFlow;

public class ResilienceOptions : IKakfaFlowOptions
{
    public RetrySettings Retry { get; set; } = new();
    public CircuitBreakerSettings CircuitBreaker { get; set; } = new();

    public bool Validate(out string errorMessage)
    {
        if (!Retry.Enabled && CircuitBreaker.Enabled)
        {
            errorMessage = $"Invalid configuration. Retry is disabled and circuit breaker is enabled.";
            return false;
        }

        var retryIsValid = Retry.Validate(out errorMessage);
        if (!retryIsValid)
        {
            return false;
        }

        return CircuitBreaker.Validate(out errorMessage);
    }
}

public class RetrySettings : IKakfaFlowOptions
{
    public bool Enabled { get; set; } = false;
    public int MedianFirstRetryDelayInMilliseconds { get; set; } = 200;
    public int MaxRetryAttempts { get; set; }
    public int PublishRetryCount { get; set; }

    public bool Validate(out string errorMessage)
    {
        errorMessage = "";

        if (Enabled)
        {
            if (MaxRetryAttempts == 0)
            {
                errorMessage = $"Invalid MaxRetryAttempts. Value: {MaxRetryAttempts}";
            }
        }

        return string.IsNullOrEmpty(errorMessage);
    }

    public bool ShouldPublishToRetry()
    {
        return Enabled && PublishRetryCount > 0;
    }

    public bool ShouldPublishToDeadLetter(string deadLetterTopic)
    {
        return !string.IsNullOrEmpty(deadLetterTopic);
    }

    public bool ShouldPublishDirectlyToDeadLetter(string deadLetterTopic)
    {
        return ShouldPublishToDeadLetter(deadLetterTopic) && (!Enabled || PublishRetryCount == 0);
    }
}

public class CircuitBreakerSettings : IKakfaFlowOptions
{
    public bool Enabled { get; set; } = false;
    public int ExceptionsAllowedBeforeBreaking { get; set; }
    public int DurationOfBreakInSeconds { get; set; }

    public bool Validate(out string errorMessage)
    {
        errorMessage = "";

        if (Enabled)
        {
            if (ExceptionsAllowedBeforeBreaking == 0)
            {
                errorMessage = $"Invalid ExceptionsAllowedBeforeBreaking. Value: {ExceptionsAllowedBeforeBreaking}";
            }

            if (DurationOfBreakInSeconds == 0)
            {
                errorMessage = $"Invalid DurationOfBreakInSeconds. Value: {DurationOfBreakInSeconds}";
            }
        }

        return string.IsNullOrEmpty(errorMessage);
    }
}
