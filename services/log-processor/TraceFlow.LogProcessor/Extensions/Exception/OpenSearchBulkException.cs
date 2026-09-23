namespace TraceFlow.LogProcessor.Services.OpenSearch;

public sealed class OpenSearchBulkException : Exception
{
    public OpenSearchBulkException(string message)
        : base(message)
    {
    }

    public OpenSearchBulkException(
        string message,
        Exception innerException)
        : base(message, innerException)
    {
    }
}