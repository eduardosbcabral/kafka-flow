using KafkaFlow.Consumer;
using KafkaFlow.Http;
using KafkaFlow.Http.Options;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KafkaFlow.Sample.Handlers;

class HttpHandler<TKey, TValue> : IKafkaHandler<TKey, TValue>
{
    private readonly ILogger<HttpHandler<TKey, TValue>> _logger;
    private readonly HttpService<TKey, TValue> _httpService;
    private readonly HttpConsumerOptions<TKey, TValue, HttpOptions> _consumerOptions;

    public HttpHandler(ILogger<HttpHandler<TKey, TValue>> logger, IOptions<HttpConsumerOptions<TKey, TValue, HttpOptions>> consumerOptions, HttpService<TKey, TValue> httpService)
    {
        _logger = logger;
        _consumerOptions = consumerOptions.Value;
        _httpService = httpService;
    }

    public Task<bool> BeforeHandleAsync(ConsumeContext<TKey, TValue> consumeContext, CancellationToken cancellationToken)
        => Task.FromResult(true);

    public async Task HandleAsync(ConsumeContext<TKey, TValue> consumeContext, CancellationToken cancellationToken)
    {
        await _httpService.SendRequestAsync(_consumerOptions.Http.EndpointUrl, consumeContext.Value, null, cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Http Handler executed.");
    }
}
