namespace KafkaFlow.Options;

public class ResilienceOptions
{
    public bool Enabled { get; set; } = false;
    public RetrySettings Retry { get; set; } = new();
    public CircuitBreakerSettings CircuitBreaker { get; set; } = new();
}

public class RetrySettings
{
    public int MedianFirstRetryDelayInMilliseconds { get; set; } = 500;
    public int MaxRetryAttempts { get; set; } = 5;
    public int PublishRetryCount { get; set; } = 3;
}

public class CircuitBreakerSettings
{
    public int ExceptionsAllowedBeforeBreaking { get; set; }
    public int DurationOfBreakInSeconds { get; set; }
}
