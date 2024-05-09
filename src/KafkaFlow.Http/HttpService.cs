﻿using System.Net.Http.Json;

namespace KafkaFlow.Http
{
    internal class HttpService(HttpClient client)
    {
        public async Task<bool> SendRequestAsync(string endpointUrl, object request, CancellationToken cancellationToken = default)
        {
            var response = await client.PostAsJsonAsync(endpointUrl, request, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return response.IsSuccessStatusCode;
        }
    }
}
