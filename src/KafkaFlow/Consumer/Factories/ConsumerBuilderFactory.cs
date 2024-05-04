using Confluent.Kafka;

using KafkaFlow.Options;

namespace KafkaFlow.Consumer.Factories;

internal class ConsumerBuilderFactory<TKey, TValue>(KafkaDeserializer<TValue> kafkaDeserializer) : IConsumerBuilderFactory<TKey, TValue>
{
    public ConsumerBuilder<TKey, TValue> Build(ConsumerOptions<TKey, TValue> consumerOptions)
    {
        return new ConsumerBuilder<TKey, TValue>(consumerOptions).SetValueDeserializer(kafkaDeserializer);
    }
}

public interface IConsumerBuilderFactory<TKey, TValue>
{
    ConsumerBuilder<TKey, TValue> Build(ConsumerOptions<TKey, TValue> consumerOptions);
}