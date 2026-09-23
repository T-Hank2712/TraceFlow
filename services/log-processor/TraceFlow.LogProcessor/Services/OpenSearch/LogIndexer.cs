using Microsoft.Extensions.Options;
using OpenSearch.Client;
using TraceFlow.LogProcessor.Configurations;
using TraceFlow.LogProcessor.Contracts;

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
    public async Task IndexAsync(IReadOnlyCollection<LogEvent> logEvents, CancellationToken cancellationToken)
    {
        if (logEvents.Count == 0) return;

        for(int attempt = 0; attempt <= _processorOptions.MaxRetries; attempt++)
        {
            try
            {
                var response = await ExecuteBulkAsync(logEvents, cancellationToken);

                if (response.IsValid)
                {
                    _logger.LogInformation("Indexed log batch into OpenSearch. Count: {Count}, Index: {Index}", logEvents.Count, _options.Index);
                    return;
                }

                _logger.LogWarning("OpenSearch bulk request failed. Attempt: {Attempt}/{MaxAttempts}. Count: {Count}",
                    attempt + 1, _processorOptions.MaxRetries + 1, logEvents.Count);
            }
            catch(Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "OpenSearch bulk request failed. Attempt: {Attempt}/{MaxAttempts}. Count: {Count}",
                attempt + 1, _processorOptions.MaxRetries + 1, logEvents.Count);
            }

            if(attempt < _processorOptions.MaxRetries)
            {
                await Task.Delay(_processorOptions.RetryBackoffMs, cancellationToken);
            }
        }

        throw new OpenSearchBulkException("OpenSearch bulk indexing failed after all retry attempts.");
    }
    public async Task<BulkResponse> ExecuteBulkAsync(IReadOnlyCollection<LogEvent> logEvents, CancellationToken cancellationToken)
    {
        return await _client.BulkAsync(
            descriptor =>
            {
                foreach (var logEvent in logEvents)
                {
                    descriptor.Index<LogEvent>(index =>
                        index
                            .Index(_options.Index)
                            .Document(logEvent));
                }

                return descriptor;
            },
            cancellationToken);
    }
}