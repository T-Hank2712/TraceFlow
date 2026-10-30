namespace TraceFlow.LogProcessor.Services.OpenSearch;

public interface IOpenSearchIndexInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken);
}
