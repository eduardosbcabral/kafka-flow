using Confluent.Kafka;

using System.Text.Json;

namespace KafkaFlow.Producer;

public sealed class KafkaSerializer<T>(JsonSerializerOptions? jsonSerializerOptions = null) : ISerializer<T>
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = jsonSerializerOptions ?? new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

#pragma warning disable CS8603 // Possible null reference return.
    public byte[] Serialize(T data, SerializationContext context)
    {
        if (typeof(T) == typeof(Null))
        {
            return default;
        }

        if (typeof(T) == typeof(Ignore))
            return default;

        if (data == null)
            return default;

        return JsonSerializer.SerializeToUtf8Bytes(data, _jsonSerializerOptions);
    }
#pragma warning restore CS8603 // Possible null reference return.
}
