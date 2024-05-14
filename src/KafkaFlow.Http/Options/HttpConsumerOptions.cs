namespace KafkaFlow.Http.Options;

public class HttpConsumerOptions<TKey, TValue, THttpOptions> : ConsumerOptions<TKey, TValue> where THttpOptions : HttpOptions
{
    public THttpOptions Http { get; set; } = default!;
}

public class HttpOptions
{
    public string EndpointUrl { get; set; } = string.Empty;
}