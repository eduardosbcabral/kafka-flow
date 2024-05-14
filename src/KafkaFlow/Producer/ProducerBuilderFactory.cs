using Confluent.Kafka;

using Microsoft.Extensions.Options;

namespace KafkaFlow.Producer;

public class ProducerBuilderFactory<TKey, TValue> : IProducerBuilderFactory<TKey, TValue>
{
    private readonly IOptions<ProducerOptions<TKey, TValue>> _producerOptions;
    private readonly KafkaSerializer<TValue> _kafkaSerializer;

    public ProducerBuilderFactory(IOptions<ProducerOptions<TKey, TValue>> producerOptions, KafkaSerializer<TValue> kafkaSerializer)
    {
        ArgumentNullException.ThrowIfNull(producerOptions, nameof(producerOptions));
        ArgumentNullException.ThrowIfNull(kafkaSerializer, nameof(kafkaSerializer));
        _producerOptions = producerOptions;
        _kafkaSerializer = kafkaSerializer;
    }

    public IProducer<TKey, TValue> Build()
    {
        var producerBuilder = new ProducerBuilder<TKey, TValue>(_producerOptions.Value).SetValueSerializer(_kafkaSerializer);
        return producerBuilder.Build();
    }
}

public interface IProducerBuilderFactory<TKey, TValue>
{
    IProducer<TKey, TValue> Build();
}
