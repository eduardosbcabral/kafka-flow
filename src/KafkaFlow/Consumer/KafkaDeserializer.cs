using Confluent.Kafka;

using System.Text.Json;

namespace KafkaFlow.Consumer;

public sealed class KafkaDeserializer<T>(JsonSerializerOptions? jsonSerializerOptions = null) : IDeserializer<T>
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = jsonSerializerOptions ?? new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

#pragma warning disable CS8603 // Possible null reference return.
    public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {
        if (typeof(T) == typeof(Null))
        {
            if (data.Length > 0)
                throw new ArgumentException("The data is null not null.");
            return default;
        }

        if (typeof(T) == typeof(Ignore))
            return default;

        return JsonSerializer.Deserialize<T>(data, _jsonSerializerOptions);
    }
#pragma warning restore CS8603 // Possible null reference return.
}
