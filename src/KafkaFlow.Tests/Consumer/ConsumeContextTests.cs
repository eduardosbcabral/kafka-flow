using Confluent.Kafka;

using KafkaFlow.Consumer;

namespace KafkaFlow.Tests.Consumer;

public class ConsumeContextTests
{
    [Fact]
    public void ShouldInstantiateCreateUsingKafkaMessage()
    {
        var message = new Message<string, string>()
        {
            Key = "key",
            Value = "value",
            Headers = [new("header", [])]
        };

        var sut = new ConsumeContext<string, string>(message);

        Assert.IsType<ConsumeContext<string, string>>(sut);
        Assert.Equal("key", sut.Key);
        Assert.Equal("value", sut.Value);
        Assert.Equal("value", sut.Message);
        Assert.NotNull(sut.Headers.GetLastBytes("header"));
    }

    [Fact]
    public void ShouldInstantiateCreateUsingProperParameters()
    {
        var sut = new ConsumeContext<string, string>("key", "value", [new("header", [])]);

        Assert.IsType<ConsumeContext<string, string>>(sut);
        Assert.Equal("key", sut.Key);
        Assert.Equal("value", sut.Value);
        Assert.Equal("value", sut.Message);
        Assert.NotNull(sut.Headers.GetLastBytes("header"));
    }

    [Fact]
    public void RetryAttemptShouldWorkSuccessfully()
    {
        var sut = new ConsumeContext<string, string>("key", "value", []);
        sut.AddRetryAttempt(1);
        Assert.Equal(1, sut.GetRetryAttempt());
    }
}
