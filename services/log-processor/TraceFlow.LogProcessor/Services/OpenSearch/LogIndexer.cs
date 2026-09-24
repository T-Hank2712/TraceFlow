using Microsoft.Extensions.Options;
using OpenSearch.Client;
using TraceFlow.LogProcessor.Configurations;
using TraceFlow.LogProcessor.Contracts;
using TraceFlow.LogProcessor.Extentions.Exceptions;

namespace TraceFlow.LogProcessor.Services.OpenSearch;

public sealed class LogIndexer : ILogIndexer
{
    private readonly IOpenSearchClient _client;
    private readonly OpenSearchOptions _options;
    private readonly ILogger<LogIndexer> _logger;
    private readonly ProcessorOptions _processorOptions;
    public LogIndexer(IOpenSearchClient client, IOptions<OpenSearchOptions> options, ILogger<LogIndexer> logger, IOptions<ProcessorOptions> processorOptions)
    {
        _client = client;
        _options = options.Value;
        _logger = logger;
        _processorOptions = processorOptions.Value;
    }
    public async Task<BulkIndexResult> IndexAsync(IReadOnlyList<LogEvent> logEvents, CancellationToken cancellationToken)
    {
        if (logEvents.Count == 0)
        {
            return new BulkIndexResult(Array.Empty<LogEvent>(), Array.Empty<LogEvent>());
        }

        for (int attempt = 0; attempt <= _processorOptions.MaxRetries; attempt++)
        {
            try
            {
                var response = await ExecuteBulkAsync(logEvents, cancellationToken);

                if (response.Items == null)
                    throw new OpenSearchBulkException("OpenSearch cluster unreachable or request failed at connection level.", response.OriginalException);

                var succeededEvents = new List<LogEvent>();
                var failedEvents = new List<LogEvent>();

                var responseItems = response.Items.ToList();

                for (int index = 0; index < responseItems.Count; index++)
                {
                    var logEvent = logEvents[index];
                    var item = responseItems[index];

                    if (item.IsValid) succeededEvents.Add(logEvent);
                    else
                    {
                        failedEvents.Add(logEvent);

                        _logger.LogWarning(
                            "OpenSearch failed to index event. EventId: {EventId}, Error: {Error}",
                            logEvent.EventId,
                            item.Error);
                    }
                }
                _logger.LogInformation(
                    "OpenSearch bulk indexing completed. Total: {Total}, Succeeded: {Succeeded}, Failed: {Failed}, Index: {Index}",
                    logEvents.Count,
                    succeededEvents.Count,
                    failedEvents.Count,
                    _options.Index);

                return new BulkIndexResult(
                    succeededEvents,
                    failedEvents);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "OpenSearch bulk request failed. Attempt: {Attempt}/{MaxAttempts}. Count: {Count}",
                attempt + 1, _processorOptions.MaxRetries + 1, logEvents.Count);
            }

            if (attempt < _processorOptions.MaxRetries)
            {
                await Task.Delay(_processorOptions.RetryBackoffMs, cancellationToken);
            }
        }

        throw new OpenSearchBulkException("OpenSearch bulk indexing failed after all retry attempts.");
    }
    public async Task<BulkResponse> ExecuteBulkAsync(IReadOnlyList<LogEvent> logEvents, CancellationToken cancellationToken)
    {
        return await _client.BulkAsync(
            b => b.Index(_options.Index)
            .IndexMany(logEvents),
            cancellationToken);
    }
}
