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
    public LogIndexer(IOpenSearchClient client, IOptions<OpenSearchOptions> options, ILogger<LogIndexer> logger)
    {
        _client = client;
        _options = options.Value;
        _logger = logger;
    }
    public async Task IndexAsync(IReadOnlyCollection<LogEvent> logEvents, CancellationToken cancellationToken)
    {
        if (logEvents.Count == 0) return;

        var response = await _client.BulkAsync(
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
        
        if (!response.IsValid)
        {
            throw new InvalidOperationException(
                $"OpenSearch bulk indexing failed: {response.DebugInformation}");
        }

        _logger.LogInformation(
            "Indexed log batch into OpenSearch. Count: {Count}, Index: {Index}",
            logEvents.Count,
            _options.Index);
    }
}