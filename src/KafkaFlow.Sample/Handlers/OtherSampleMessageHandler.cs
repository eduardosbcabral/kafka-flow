using KafkaFlow.Consumer;
using KafkaFlow.Sample.Messages;

namespace KafkaFlow.Sample.Handlers;

class OtherSampleMessageHandler : IKafkaHandler<string, OtherSampleMessage>
{
    private readonly ILogger<OtherSampleMessageHandler> _logger;

    public OtherSampleMessageHandler(ILogger<OtherSampleMessageHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(ConsumeContext<string, OtherSampleMessage> consumeContext, CancellationToken cancellationToken)
    {
        var message = consumeContext.Message;

        _logger.LogInformation(
            $"Other sample message received with key: {message.Key} and value: {message.SomeOtherProperty}");

        return Task.CompletedTask;
    }
}