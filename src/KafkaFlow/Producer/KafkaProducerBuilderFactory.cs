using Confluent.Kafka;

using KafkaFlow.Options;

using Microsoft.Extensions.Options;

namespace KafkaFlow.Producer;

public class KafkaProducerBuilderFactory<TKey, TValue>(IOptions<ProducerOptions<TKey, TValue>> producerOptions, KafkaSerializer<TValue> kafkaSerializer) : IKafkaProducerBuilderFactory<TKey, TValue>
{
    public IProducer<TKey, TValue> Build()
    {
        var producerBuilder = new ProducerBuilder<TKey, TValue>(producerOptions.Value).SetValueSerializer(kafkaSerializer);
        return producerBuilder.Build();
    }
}

public interface IKafkaProducerBuilderFactory<TKey, TValue>
{
    IProducer<TKey, TValue> Build();
}
