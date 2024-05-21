using System.Net.Http.Json;
using System.Text.Json;

namespace KafkaFlow.Http
{
    public class HttpService<TKey, TValue>(HttpClient client)
    {
        public async Task<bool> SendRequestAsync(string endpointUrl, object request, Dictionary<string, string>? headers = null, JsonSerializerOptions? jsonSerializerOptions = null, CancellationToken cancellationToken = default)
        {
            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, endpointUrl);
            if (headers is not null)
            {
                foreach (var header in headers)
                {
                    requestMessage.Headers.Add(header.Key, header.Value);
                }
            }

            requestMessage.Content = JsonContent.Create(request, options: jsonSerializerOptions);

            var response = await client.SendAsync(requestMessage, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return response.IsSuccessStatusCode;
        }
    }
}
