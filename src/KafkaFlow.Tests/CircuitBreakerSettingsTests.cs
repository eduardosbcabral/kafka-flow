namespace KafkaFlow.Tests;

public class CircuitBreakerSettingsTests
{
    [Fact]
    public void Validate_EnabledWithValidValues_ReturnsTrue()
    {
        // Arrange
        var settings = new CircuitBreakerSettings
        {
            Enabled = true,
            ExceptionsAllowedBeforeBreaking = 3,
            DurationOfBreakInSeconds = 10
        };

        // Act
        var result = settings.Validate(out var errorMessage);

        // Assert
        Assert.True(result);
        Assert.Empty(errorMessage);
    }

    [Fact]
    public void Validate_EnabledWithZeroExceptionsAllowedBeforeBreaking_ReturnsFalse()
    {
        // Arrange
        var settings = new CircuitBreakerSettings
        {
            Enabled = true,
            ExceptionsAllowedBeforeBreaking = 0,
            DurationOfBreakInSeconds = 10
        };

        // Act
        var result = settings.Validate(out var errorMessage);

        // Assert
        Assert.False(result);
        Assert.Equal($"Invalid ExceptionsAllowedBeforeBreaking. Value: {settings.ExceptionsAllowedBeforeBreaking}", errorMessage);
    }

    [Fact]
    public void Validate_EnabledWithZeroDurationOfBreakInSeconds_ReturnsFalse()
    {
        // Arrange
        var settings = new CircuitBreakerSettings
        {
            Enabled = true,
            ExceptionsAllowedBeforeBreaking = 3,
            DurationOfBreakInSeconds = 0
        };

        // Act
        var result = settings.Validate(out var errorMessage);

        // Assert
        Assert.False(result);
        Assert.Equal($"Invalid DurationOfBreakInSeconds. Value: {settings.DurationOfBreakInSeconds}", errorMessage);
    }

    [Fact]
    public void Validate_Disabled_ReturnsTrue()
    {
        // Arrange
        var settings = new CircuitBreakerSettings
        {
            Enabled = false,
            ExceptionsAllowedBeforeBreaking = 0,
            DurationOfBreakInSeconds = 0
        };

        // Act
        var result = settings.Validate(out var errorMessage);

        // Assert
        Assert.True(result);
        Assert.Empty(errorMessage);
    }
}
