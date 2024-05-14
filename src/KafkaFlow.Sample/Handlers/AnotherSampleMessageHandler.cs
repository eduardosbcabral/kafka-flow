using KafkaFlow.Consumer;
using KafkaFlow.Sample.Messages;

namespace KafkaFlow.Sample.Handlers;

class AnotherSampleMessageHandler : IKafkaHandler<string, AnotherSampleMessage>
{
    private readonly ILogger<AnotherSampleMessageHandler> _logger;

    public AnotherSampleMessageHandler(ILogger<AnotherSampleMessageHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(ConsumeContext<string, AnotherSampleMessage> consumeContext, CancellationToken cancellationToken)
    {
        var message = consumeContext.Message;

        _logger.LogInformation(
            $"Another sample message received with key: {message.Key} and value: {message.AnotherProperty}");

        return Task.CompletedTask;
    }
}
