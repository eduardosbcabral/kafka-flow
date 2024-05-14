using Confluent.Kafka;

namespace KafkaFlow;

public class ConsumerOptions<TKey, TValue> : ConsumerConfig
{
    public string Name { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string Retry { get; set; } = string.Empty;
    public string DeadLetter { get; set; } = string.Empty;

    public ResilienceOptions Resilience { get; set; } = new();

    public bool Validate(out string errorMessage)
    {
        if (string.IsNullOrEmpty(Name))
        {
            errorMessage = $"{GetType().Name} - Name is required";
            return false;
        }

        if (string.IsNullOrEmpty(Topic))
        {
            errorMessage = $"{GetType().Name} - Topic is required";
            return false;
        }

        if (string.IsNullOrEmpty(Retry) && Resilience.Retry.Enabled)
        {
            errorMessage = $"{GetType().Name} - Retry is required";
            return false;
        }

        if (string.IsNullOrEmpty(DeadLetter) && Resilience.Retry.Enabled)
        {
            errorMessage = $"{GetType().Name} - DeadLetter is required";
            return false;
        }

        return Resilience.Validate(out errorMessage);
    }
}