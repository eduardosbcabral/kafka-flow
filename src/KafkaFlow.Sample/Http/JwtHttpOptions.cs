using KafkaFlow.Http.Options;

namespace KafkaFlow.Sample.Http;

internal class JwtHttpOptions : HttpOptions
{
    public string AuthenticationEndpointUrl { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}
