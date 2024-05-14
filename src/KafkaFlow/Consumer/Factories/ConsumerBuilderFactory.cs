using Confluent.Kafka;

using Microsoft.Extensions.Options;

namespace KafkaFlow.Consumer.Factories;

public class ConsumerBuilderFactory<TKey, TValue> : IConsumerBuilderFactory<TKey, TValue>
{
    private readonly KafkaDeserializer<TValue> _kafkaDeserializer;
    private readonly ConsumerOptions<TKey, TValue> _consumerOptions;

    public ConsumerBuilderFactory(IOptions<ConsumerOptions<TKey, TValue>> consumerOptions, KafkaDeserializer<TValue> kafkaDeserializer)
    {
        ArgumentNullException.ThrowIfNull(consumerOptions, nameof(consumerOptions));
        ArgumentNullException.ThrowIfNull(kafkaDeserializer, nameof(kafkaDeserializer));
        _kafkaDeserializer = kafkaDeserializer;
        _consumerOptions = consumerOptions.Value;
    }

    public ConsumerBuilder<TKey, TValue> Build()
    {
        return new ConsumerBuilder<TKey, TValue>(_consumerOptions).SetValueDeserializer(_kafkaDeserializer);
    }
}

public interface IConsumerBuilderFactory<TKey, TValue>
{
    ConsumerBuilder<TKey, TValue> Build();
}