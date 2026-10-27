namespace TraceFlow.LogProcessor.Extentions.Exceptions;

public sealed class OpenSearchBulkResponseException : Exception
{
    public OpenSearchBulkResponseException(string message)
        : base(message)
    {
    }

    public OpenSearchBulkResponseException(
        string message,
        Exception innerException)
        : base(message, innerException)
    {
    }
}