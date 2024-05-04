using KafkaFlow.Consumer;
using KafkaFlow.Sample.Messages;

namespace KafkaFlow.Sample.Handlers;

class SampleMessageHandler(ILogger<SampleMessageHandler> logger) : IKafkaHandler<string, KafkaMessage<SampleMessage>>
{
    public Task HandleAsync(ConsumeContext<string, KafkaMessage<SampleMessage>> consumeContext, CancellationToken cancellationToken)
    {
        var message = consumeContext.Message.Payload.Data;

        if (message.Key?.Contains("error") == true)
        {
            throw new Exception("Key property has an error.");
        }

        logger.LogInformation(
            $"Sample message received with key: {message.Key} and value: {message.SomeProperty}");

        return Task.CompletedTask;
    }
}
