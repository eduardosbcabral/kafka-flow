using Confluent.Kafka;

using MediatR;

using System.Text;

namespace KafkaFlow.Consumer;

public class ConsumeContext<TKey, TValue> : Message<TKey, TValue>, IRequest
{
    public const string RETRY_COUNT_KEY = "retry-count";

    public ConsumeContext(Message<TKey, TValue> message)
    {
        Key = message.Key;
        Value = message.Value;
        Headers = message.Headers;
    }

    public ConsumeContext(TKey key, TValue value, Headers headers)
    {
        Key = key;
        Value = value;
        Headers = headers;
    }

    public int GetRetryAttempt()
    {
        try
        {
            var messageTypeEncoded = Headers.GetLastBytes(RETRY_COUNT_KEY);
            var parsed = int.TryParse(messageTypeEncoded, out var retryAttempt);
            return parsed switch
            {
                true => retryAttempt,
                false => 0
            };
        } 
        catch (KeyNotFoundException)
        {
            return 0;
        }
    }

    public void AddRetryAttempt(int retryCount)
    {
        Headers.Remove(RETRY_COUNT_KEY);
        Headers.Add(new(RETRY_COUNT_KEY, Encoding.UTF8.GetBytes(retryCount.ToString())));
    }

    public TValue Message => Value;
}