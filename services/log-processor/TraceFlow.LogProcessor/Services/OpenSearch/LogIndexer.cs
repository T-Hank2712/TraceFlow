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
    public async Task<BulkIndexResult> IndexAsync(
        IReadOnlyList<LogEvent> logEvents,
        CancellationToken cancellationToken)
    {
        if (logEvents.Count == 0)
        {
            return new BulkIndexResult(
                Array.Empty<LogEvent>(),
                Array.Empty<LogEvent>());
        }

        for (int attempt = 0; attempt <= _processorOptions.MaxRetries; attempt++)
        {
            try
            {
                var response = await ExecuteBulkAsync(logEvents, cancellationToken);

                if (!response.IsValid)
                {
                    const string message =
                        "OpenSearch bulk request returned an invalid response.";

                    if (response.OriginalException is not null)
                    {
                        throw new OpenSearchBulkResponseException(
                            message,
                            response.OriginalException);
                    }

                    throw new OpenSearchBulkResponseException(message);
                }

                if (response.Items is null)
                {
                    const string message =
                        "OpenSearch bulk response did not contain indexing results.";

                    if (response.OriginalException is not null)
                    {
                        throw new OpenSearchBulkResponseException(
                            message,
                            response.OriginalException);
                    }

                    throw new OpenSearchBulkResponseException(message);
                }

                var responseItems = response.Items.ToList();

                if (responseItems.Count != logEvents.Count)
                {
                    throw new OpenSearchBulkResponseException(
                        "OpenSearch bulk response item count does not match " +
                        $"the submitted event count. Submitted: {logEvents.Count}, " +
                        $"Received: {responseItems.Count}.");
                }

                var succeededEvents = new List<LogEvent>();
                var failedEvents = new List<LogEvent>();

                for (var index = 0; index < responseItems.Count; index++)
                {
                    var logEvent = logEvents[index];
                    var item = responseItems[index];

                    if (item.IsValid)
                    {
                        succeededEvents.Add(logEvent);
                    }
                    else
                    {
                        failedEvents.Add(logEvent);
                    }
                }

                return new BulkIndexResult(succeededEvents, failedEvents);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                _logger.LogWarning(
                    exception,
                    "OpenSearch bulk request failed or returned an incomplete response. " +
                    "Attempt: {Attempt}/{MaxAttempts}. Count: {Count}",
                    attempt + 1,
                    _processorOptions.MaxRetries + 1,
                    logEvents.Count);
            }

            if (attempt < _processorOptions.MaxRetries)
            {
                await Task.Delay(
                    _processorOptions.RetryBackoffMs,
                    cancellationToken);
            }
        }

        throw new OpenSearchBulkException(
            "OpenSearch bulk indexing failed after all retry attempts.");
    }
    public async Task<BulkResponse> ExecuteBulkAsync(IReadOnlyList<LogEvent> logEvents, CancellationToken cancellationToken)
    {
        return await _client.BulkAsync(
            bulk => bulk
                .Index(_options.Index)
                .IndexMany(
                    logEvents,
                    (operation, logEvent) => operation
                        .Id(logEvent.EventId)),
            cancellationToken);
    }
}
