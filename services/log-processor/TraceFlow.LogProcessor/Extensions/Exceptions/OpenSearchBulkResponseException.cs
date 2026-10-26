namespace TraceFlow.LogProcessor.Extentions.Exceptions;

public sealed class OpenSearchBulkResponseException : Exception
{
    public OpenSearchBulkResponseException(string message)
        : base(message)
    {
    }
}
