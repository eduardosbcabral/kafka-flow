using KafkaFlow.Http.Options;

using Microsoft.Extensions.Options;

using System.Net.Http.Json;

namespace KafkaFlow.Http
{
    internal class HttpService<TKey, TValue>(IOptions<HttpConsumerOptions<TKey, TValue>> httpConsumerOptions, HttpClient client)
    {
        public async Task<bool> SendRequestAsync(TValue request, CancellationToken cancellationToken = default)
        {
            var url = httpConsumerOptions.Value.Http.EndpointUrl;
            var response = await client.PostAsJsonAsync(url, request, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return response.IsSuccessStatusCode;
        }
    }
}
