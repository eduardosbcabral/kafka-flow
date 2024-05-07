using KafkaFlow.Consumer;
using KafkaFlow.Http;

using Microsoft.Extensions.Logging;

namespace KafkaFlow.Sample.Handlers;

class HttpHandler<TKey, TValue> : IKafkaHandler<TKey, TValue>
{
    private readonly ILogger<HttpHandler<TKey, TValue>> _logger;
    private readonly HttpService<TKey, TValue> _httpService;

    public HttpHandler(ILogger<HttpHandler<TKey, TValue>> logger, HttpService<TKey, TValue> httpService)
    {
        _logger = logger;
        _httpService = httpService;
    }

    public async Task HandleAsync(ConsumeContext<TKey, TValue> consumeContext, CancellationToken cancellationToken)
    {
        var message = consumeContext.Value;
        await _httpService.SendRequestAsync(message).ConfigureAwait(false);
        _logger.LogInformation("Http Handler executed.");
    }
}
