namespace KafkaFlow.Tests;

public class ResilienceOptionsTests
{
    [Fact]
    public void Validate_RetryDisabledCircuitBreakerEnabled_ReturnsFalse()
    {
        // Arrange
        var options = new ResilienceOptions
        {
            Retry = new RetrySettings { Enabled = false },
            CircuitBreaker = new CircuitBreakerSettings { Enabled = true }
        };

        // Act
        var isValid = options.Validate(out var errorMessage);

        // Assert
        Assert.False(isValid);
        Assert.Equal("Invalid configuration. Retry is disabled and circuit breaker is enabled.", errorMessage);
    }
}
