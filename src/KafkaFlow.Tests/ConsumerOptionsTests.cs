namespace KafkaFlow.Tests;

public class ConsumerOptionsTests
{
    [Fact]
    public void Validate_ValidOptionsWithFullResilienceEnabled_ReturnsTrue()
    {
        // Arrange
        var options = new ConsumerOptions<string, string>
        {
            Name = "TestConsumer",
            Topic = "test-topic",
            Retry = "retry-topic",
            DeadLetter = "dead-letter-topic",
            Resilience = new ResilienceOptions
            {
                Retry = new RetrySettings
                {
                    Enabled = true,
                    MaxRetryAttempts = 3,
                    PublishRetryCount = 2
                },
                CircuitBreaker = new CircuitBreakerSettings
                {
                    Enabled = true,
                    ExceptionsAllowedBeforeBreaking = 5,
                    DurationOfBreakInSeconds = 30
                }
            }
        };

        // Act
        var isValid = options.Validate(out var errorMessage);

        // Assert
        Assert.True(isValid);
        Assert.Empty(errorMessage);
    }

    [Fact]
    public void Validate_WithoutResilience_ReturnsTrue()
    {
        // Arrange
        var options = new ConsumerOptions<string, string>
        {
            Name = "TestConsumer",
            Topic = "test-topic",
            Resilience = new ResilienceOptions
            {
                Retry = new RetrySettings
                {
                    Enabled = false,
                },
                CircuitBreaker = new CircuitBreakerSettings
                {
                    Enabled = false,
                }
            }
        };

        // Act
        var isValid = options.Validate(out var errorMessage);

        // Assert
        Assert.True(isValid);
        Assert.Empty(errorMessage);
    }

    [Fact]
    public void Validate_MissingName_ReturnsFalse()
    {
        // Arrange
        var options = new ConsumerOptions<string, string>
        {
            Topic = "test-topic",
            Retry = "retry-topic",
            DeadLetter = "dead-letter-topic"
        };

        // Act
        var isValid = options.Validate(out var errorMessage);

        // Assert
        Assert.False(isValid);
        Assert.Contains("Name is required", errorMessage);
    }

    [Fact]
    public void Validate_MissingTopic_ReturnsFalse()
    {
        // Arrange
        var options = new ConsumerOptions<string, string>
        {
            Name = "TestConsumer",
            Retry = "retry-topic",
            DeadLetter = "dead-letter-topic"
        };

        // Act
        var isValid = options.Validate(out var errorMessage);

        // Assert
        Assert.False(isValid);
        Assert.Contains("Topic is required", errorMessage);
    }

    [Fact]
    public void Validate_RetryEnabledButMissingRetryTopic_ReturnsFalse()
    {
        // Arrange
        var options = new ConsumerOptions<string, string>
        {
            Name = "TestConsumer",
            Topic = "test-topic",
            DeadLetter = "dead-letter-topic",
            Resilience = new ResilienceOptions
            {
                Retry = new RetrySettings { Enabled = true }
            }
        };

        // Act
        var isValid = options.Validate(out var errorMessage);

        // Assert
        Assert.False(isValid);
        Assert.Contains("Retry is required", errorMessage);
    }

    [Fact]
    public void Validate_RetryEnabledButMissingDeadLetterTopic_ReturnsFalse()
    {
        // Arrange
        var options = new ConsumerOptions<string, string>
        {
            Name = "TestConsumer",
            Topic = "test-topic",
            Retry = "retry-topic",
            Resilience = new ResilienceOptions
            {
                Retry = new RetrySettings { Enabled = true }
            }
        };

        // Act
        var isValid = options.Validate(out var errorMessage);

        // Assert
        Assert.False(isValid);
        Assert.Contains("DeadLetter is required", errorMessage);
    }
}
