using Confluent.Kafka;

using System.ComponentModel.DataAnnotations;

namespace KafkaFlow.Options;

public class ConsumerOptions<TKey, TValue> : ConsumerConfig
{
    [Required]
    public string Name { get; set; } = string.Empty;
    [Required]
    public string Topic { get; set; } = string.Empty;
    public string Retry { get; set; } = string.Empty;
    public string DeadLetter { get; set; } = string.Empty;

    public ResilienceOptions Resilience { get; set; } = new();
}