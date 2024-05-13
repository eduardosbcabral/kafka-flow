using KafkaFlow.Http.Options;

using Microsoft.Extensions.Options;

using System.Net.Http.Headers;
using System.Text.Json;

namespace KafkaFlow.Sample.Http;

internal class JwtAuthenticationHandler<TKey, TValue> : DelegatingHandler
{
    private static string Token = string.Empty;
    private static DateTime TokenExpiryTime;
    private static readonly SemaphoreSlim TokenRefreshSemaphore = new(1, 1);

    private readonly JwtHttpOptions _http;
    private readonly JsonSerializerOptions? _jsonSerializerOptions;

    public JwtAuthenticationHandler(
        IOptions<HttpConsumerOptions<TKey, TValue, JwtHttpOptions>> httpConsumerOptions,
        JsonSerializerOptions? jsonSerializerOptions = null)
    {
        _http = httpConsumerOptions.Value.Http;
        _jsonSerializerOptions = jsonSerializerOptions ?? new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(Token) || DateTime.UtcNow >= TokenExpiryTime)
        {
            await RefreshTokenIfNeededAsync(cancellationToken);
        }

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Token);
        return await base.SendAsync(request, cancellationToken);
    }

    private async Task RefreshTokenIfNeededAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(Token) || DateTime.UtcNow >= TokenExpiryTime)
        {
            await TokenRefreshSemaphore.WaitAsync(cancellationToken);
            try
            {
                // Double-check the token status to ensure it wasn't refreshed by another thread while waiting
                if (string.IsNullOrEmpty(Token) || DateTime.UtcNow >= TokenExpiryTime)
                {
                    await FetchTokenAsync(cancellationToken);
                }
            }
            finally
            {
                TokenRefreshSemaphore.Release();
            }
        }
    }

    private async Task FetchTokenAsync(CancellationToken cancellationToken)
    {
        using var client = new HttpClient();

        var content = new FormUrlEncodedContent(
        [
            new("client_id", _http.ClientId),
            new("client_secret", _http.ClientSecret),
            new("grant_type", "client_credentials")
        ]);

        var response = await client.PostAsync(_http.AuthenticationEndpointUrl, content, cancellationToken).ConfigureAwait(false);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent, _jsonSerializerOptions);
        if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken) || tokenResponse.ExpiresIn == 0)
        {
            throw new HttpRequestException("Invalid authentication response.");
        }

        Token = tokenResponse.AccessToken;
        TokenExpiryTime = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 60); // Subtract 60 seconds to ensure there's a buffer before expiry
    }

    private class TokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
    }
}
