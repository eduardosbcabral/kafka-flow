namespace KafkaFlow.Tests;

public class RetrySettingsTests
{
    [Fact]
    public void Validate_EnabledWithValidValues_ReturnsTrue()
    {
        // Arrange
        var settings = new RetrySettings
        {
            Enabled = true,
            MaxRetryAttempts = 3,
            PublishRetryCount = 2
        };

        // Act
        var result = settings.Validate(out var errorMessage);

        // Assert
        Assert.True(result);
        Assert.Empty(errorMessage);
    }

    [Fact]
    public void Validate_EnabledWithZeroMaxRetryAttempts_ReturnsFalse()
    {
        // Arrange
        var settings = new RetrySettings
        {
            Enabled = true,
            MaxRetryAttempts = 0,
            PublishRetryCount = 2
        };

        // Act
        var result = settings.Validate(out var errorMessage);

        // Assert
        Assert.False(result);
        Assert.Equal($"Invalid MaxRetryAttempts. Value: {settings.MaxRetryAttempts}", errorMessage);
    }

    [Fact]
    public void ShouldPublishToRetry_EnabledAndPublishRetryCountGreaterThanZero_ReturnsTrue()
    {
        // Arrange
        var settings = new RetrySettings
        {
            Enabled = true,
            PublishRetryCount = 2
        };

        // Act
        var result = settings.ShouldPublishToRetry();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ShouldPublishToRetry_Disabled_ReturnsFalse()
    {
        // Arrange
        var settings = new RetrySettings
        {
            Enabled = false,
            PublishRetryCount = 2
        };

        // Act
        var result = settings.ShouldPublishToRetry();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ShouldPublishToRetry_EnabledButZeroPublishRetryCount_ReturnsFalse()
    {
        // Arrange
        var settings = new RetrySettings
        {
            Enabled = true,
            PublishRetryCount = 0
        };

        // Act
        var result = settings.ShouldPublishToRetry();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ShouldPublishToDeadLetter_DeadLetterTopicNotEmpty_ReturnsTrue()
    {
        // Arrange
        var settings = new RetrySettings();
        var deadLetterTopic = "dead-letter-topic";

        // Act
        var result = settings.ShouldPublishToDeadLetter(deadLetterTopic);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ShouldPublishToDeadLetter_DeadLetterTopicEmpty_ReturnsFalse()
    {
        // Arrange
        var settings = new RetrySettings();
        var deadLetterTopic = "";

        // Act
        var result = settings.ShouldPublishToDeadLetter(deadLetterTopic);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ShouldPublishDirectlyToDeadLetter_DeadLetterTopicNotEmptyAndRetryDisabled_ReturnsTrue()
    {
        // Arrange
        var settings = new RetrySettings
        {
            Enabled = false
        };
        var deadLetterTopic = "dead-letter-topic";

        // Act
        var result = settings.ShouldPublishDirectlyToDeadLetter(deadLetterTopic);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ShouldPublishDirectlyToDeadLetter_DeadLetterTopicNotEmptyAndPublishRetryCountIsZero_ReturnsTrue()
    {
        // Arrange
        var settings = new RetrySettings
        {
            Enabled = true,
            PublishRetryCount = 0
        };
        var deadLetterTopic = "dead-letter-topic";

        // Act
        var result = settings.ShouldPublishDirectlyToDeadLetter(deadLetterTopic);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ShouldPublishDirectlyToDeadLetter_DeadLetterTopicEmpty_ReturnsFalse()
    {
        // Arrange
        var settings = new RetrySettings
        {
            Enabled = true,
            PublishRetryCount = 2
        };
        var deadLetterTopic = "";

        // Act
        var result = settings.ShouldPublishDirectlyToDeadLetter(deadLetterTopic);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ShouldPublishDirectlyToDeadLetter_DeadLetterTopicNotEmptyAndRetryEnabledAndPublishRetryCountGreaterThanZero_ReturnsFalse()
    {
        // Arrange
        var settings = new RetrySettings
        {
            Enabled = true,
            PublishRetryCount = 2
        };
        var deadLetterTopic = "dead-letter-topic";

        // Act
        var result = settings.ShouldPublishDirectlyToDeadLetter(deadLetterTopic);

        // Assert
        Assert.False(result);
    }
}
