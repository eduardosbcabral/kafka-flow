using KafkaFlow.Options;

namespace KafkaFlow.Http.Options;

public class HttpConsumerOptions<TKey, TValue> : ConsumerOptions<TKey, TValue>
{
    public HttpOptions Http { get; set; } = new();
}

public class HttpOptions
{
    public string EndpointUrl { get; set; } = string.Empty;
    public string AuthenticationEndpointUrl { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}