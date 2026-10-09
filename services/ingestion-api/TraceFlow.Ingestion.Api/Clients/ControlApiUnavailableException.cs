namespace TraceFlow.Ingestion.Api.Clients;

public sealed class ControlApiUnavailableException : Exception
{
    public ControlApiUnavailableException(string message)
        : base(message)
    {
    }

    public ControlApiUnavailableException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
