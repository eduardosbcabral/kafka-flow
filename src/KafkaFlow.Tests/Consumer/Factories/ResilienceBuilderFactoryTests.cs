using KafkaFlow.Consumer;
using KafkaFlow.Consumer.Factories;
using KafkaFlow.Producer;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

using Polly;
using Polly.CircuitBreaker;
using Polly.Fallback;
using Polly.Retry;
using Polly.Wrap;

namespace KafkaFlow.Tests.Consumer.Factories;

public class ResilienceBuilderFactoryTests
{
    [Fact]
    public void BuildAsyncPolicy_WithNoResilience_ReturnsBasicFallbackPolicy()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ResilienceBuilderFactory<string, string>>>();
        var consumerOptions = Options.Create(new ConsumerOptions<string, string>
        {
            Name = "TestConsumer",
            Topic = "TestTopic",
            Resilience = new ResilienceOptions
            {
                Retry = new RetrySettings
                {
                    Enabled = false,
                },
                CircuitBreaker = new CircuitBreakerSettings
                {
                    Enabled = false
                }
            }
        });
        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();

        // Act
        var resilienceBuilderFactory = new ResilienceBuilderFactory<string, string>(
            loggerMock.Object,
            consumerOptions,
            serviceScopeFactoryMock.Object);

        var asyncPolicy = resilienceBuilderFactory.GetAsyncPolicy();

        // Assert
        Assert.IsType<AsyncFallbackPolicy>(asyncPolicy);
    }

    [Fact]
    public void BuildAsyncPolicy_WithResilienceRetryWithoutAttempt_ReturnsBasicFallbackPolicy()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ResilienceBuilderFactory<string, string>>>();
        var consumerOptions = Options.Create(new ConsumerOptions<string, string>
        {
            Name = "TestConsumer",
            Topic = "TestTopic",
            Resilience = new ResilienceOptions
            {
                Retry = new RetrySettings
                {
                    Enabled = true,
                    MaxRetryAttempts = 0
                },
                CircuitBreaker = new CircuitBreakerSettings
                {
                    Enabled = false
                }
            }
        });
        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();

        // Act
        var resilienceBuilderFactory = new ResilienceBuilderFactory<string, string>(
            loggerMock.Object,
            consumerOptions,
            serviceScopeFactoryMock.Object);

        var asyncPolicy = resilienceBuilderFactory.GetAsyncPolicy();

        // Assert
        Assert.IsType<AsyncFallbackPolicy>(asyncPolicy);
    }

    [Fact]
    public void BuildAsyncPolicy_WithResilienceRetryAttempts_ReturnsWrapPolicy()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ResilienceBuilderFactory<string, string>>>();
        var maxRetryAttempts = 5;
        var medianFirstRetryDelayInMilliseconds = 100;
        var consumerOptions = Options.Create(new ConsumerOptions<string, string>
        {
            Name = "TestConsumer",
            Topic = "TestTopic",
            Resilience = new ResilienceOptions
            {
                Retry = new RetrySettings
                {
                    Enabled = true,
                    MaxRetryAttempts = maxRetryAttempts,
                    MedianFirstRetryDelayInMilliseconds = medianFirstRetryDelayInMilliseconds,
                },
            }
        });
        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();

        // Act
        var resilienceBuilderFactory = new ResilienceBuilderFactory<string, string>(
            loggerMock.Object,
            consumerOptions,
            serviceScopeFactoryMock.Object);

        var asyncPolicy = resilienceBuilderFactory.GetAsyncPolicy();

        var wrapPolicy = (AsyncPolicyWrap)asyncPolicy;

        // Assert
        Assert.IsType<AsyncPolicyWrap>(asyncPolicy);
        Assert.Equal(2, wrapPolicy.GetPolicies().Count());
        Assert.NotNull(wrapPolicy.GetPolicy<AsyncRetryPolicy>());
    }

    [Fact]
    public async Task ExecuteRetryPolicy_ShouldRetryCorrectly()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ResilienceBuilderFactory<string, string>>>();
        var maxRetryAttempts = 5;
        var medianFirstRetryDelayInMilliseconds = 100;
        var executionCount = 0;

        var consumerOptions = Options.Create(new ConsumerOptions<string, string>
        {
            Name = "TestConsumer",
            Topic = "TestTopic",
            Resilience = new ResilienceOptions
            {
                Retry = new RetrySettings
                {
                    Enabled = true,
                    MaxRetryAttempts = maxRetryAttempts,
                    MedianFirstRetryDelayInMilliseconds = medianFirstRetryDelayInMilliseconds,
                },
            }
        });
        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();

        // Act
        var resilienceBuilderFactory = new ResilienceBuilderFactory<string, string>(
            loggerMock.Object,
            consumerOptions,
            serviceScopeFactoryMock.Object);

        var asyncPolicy = resilienceBuilderFactory.GetAsyncPolicy();
        var wrapPolicy = (AsyncPolicyWrap)asyncPolicy;
        var retryPolicy = wrapPolicy.GetPolicy<AsyncRetryPolicy>();

        try
        {
            await retryPolicy.ExecuteAsync(async () =>
            {
                executionCount++;
                throw new Exception("Simulated failure");
            });
        }
        catch { }

        // Assert
        Assert.Equal(maxRetryAttempts + 1, executionCount);
    }

    [Fact]
    public void BuildAsyncPolicy_WithOnlyResilienceCircuitBreaker_ReturnsBasicFallbackPolicy()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ResilienceBuilderFactory<string, string>>>();
        var consumerOptions = Options.Create(new ConsumerOptions<string, string>
        {
            Name = "TestConsumer",
            Topic = "TestTopic",
            Resilience = new ResilienceOptions
            {
                CircuitBreaker = new CircuitBreakerSettings
                {
                    Enabled = true,
                    DurationOfBreakInSeconds = 1,
                    ExceptionsAllowedBeforeBreaking = 2
                },
            }
        });
        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();

        // Act
        var resilienceBuilderFactory = new ResilienceBuilderFactory<string, string>(
            loggerMock.Object,
            consumerOptions,
            serviceScopeFactoryMock.Object);

        var asyncPolicy = resilienceBuilderFactory.GetAsyncPolicy();

        Assert.IsType<AsyncFallbackPolicy>(asyncPolicy);
    }

    [Fact]
    public void BuildAsyncPolicy_WithResilienceRetryAndCircuitBreaker_ReturnsWrapPolicy()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ResilienceBuilderFactory<string, string>>>();
        var consumerOptions = Options.Create(new ConsumerOptions<string, string>
        {
            Name = "TestConsumer",
            Topic = "TestTopic",
            Resilience = new ResilienceOptions
            {
                Retry = new RetrySettings
                {
                    Enabled = true,
                    MaxRetryAttempts = 2,
                    MedianFirstRetryDelayInMilliseconds = 100,
                },
                CircuitBreaker = new CircuitBreakerSettings
                {
                    Enabled = true,
                    DurationOfBreakInSeconds = 1,
                    ExceptionsAllowedBeforeBreaking = 2
                },
            }
        });
        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();

        // Act
        var resilienceBuilderFactory = new ResilienceBuilderFactory<string, string>(
            loggerMock.Object,
            consumerOptions,
            serviceScopeFactoryMock.Object);

        var asyncPolicy = resilienceBuilderFactory.GetAsyncPolicy();

        var wrapPolicy = (AsyncPolicyWrap)asyncPolicy;

        // Assert
        Assert.IsType<AsyncPolicyWrap>(asyncPolicy);
        Assert.Equal(3, wrapPolicy.GetPolicies().Count());
        Assert.NotNull(wrapPolicy.GetPolicy<AsyncRetryPolicy>());
        Assert.NotNull(wrapPolicy.GetPolicy<AsyncCircuitBreakerPolicy>());
    }

    [Fact]
    public async Task ExecuteRetryAndCircuitBreakerPolicy_ShouldOpenCircuitSuccessfully()
    {
        // Arrange
        var maxRetryAttempts = 1;
        var executionCount = 0;
        var loggerMock = new Mock<ILogger<ResilienceBuilderFactory<string, string>>>();
        var consumerOptions = Options.Create(new ConsumerOptions<string, string>
        {
            Name = "TestConsumer",
            Topic = "TestTopic",
            Resilience = new ResilienceOptions
            {
                Retry = new RetrySettings
                {
                    Enabled = true,
                    MaxRetryAttempts = maxRetryAttempts,
                    MedianFirstRetryDelayInMilliseconds = 100,
                },
                CircuitBreaker = new CircuitBreakerSettings
                {
                    Enabled = true,
                    DurationOfBreakInSeconds = 5,
                    ExceptionsAllowedBeforeBreaking = 1
                },
            }
        });

        var serviceProvider = new Mock<IServiceProvider>();
        var serviceScope = new Mock<IServiceScope>();
        serviceScope.Setup(x => x.ServiceProvider).Returns(serviceProvider.Object);

        var serviceScopeFactory = new Mock<IServiceScopeFactory>();
        serviceScopeFactory
            .Setup(x => x.CreateScope())
            .Returns(serviceScope.Object);
        // Act
        var resilienceBuilderFactory = new ResilienceBuilderFactory<string, string>(
            loggerMock.Object,
            consumerOptions,
            serviceScopeFactory.Object);

        var asyncPolicy = resilienceBuilderFactory.GetAsyncPolicy();

        var wrapPolicy = (AsyncPolicyWrap)asyncPolicy;

        var retryPolicy = wrapPolicy.GetPolicy<AsyncRetryPolicy>();

        var dictionary = new Dictionary<string, object>()
        {
            { "consumeContext", new ConsumeContext<string, string>("", "", []) }
        };

        try
        {
            await wrapPolicy.ExecuteAsync(async (context) =>
            {
                executionCount++;
                throw new Exception("Simulated failure");
            }, dictionary);
        } catch (Exception) { }
        // Assert
        Assert.Equal(CircuitState.Open, resilienceBuilderFactory.GetCircuitBreakerPolicy().CircuitState);
    }
}
