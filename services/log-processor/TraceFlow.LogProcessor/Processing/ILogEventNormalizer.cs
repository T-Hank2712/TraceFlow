namespace TraceFlow.LogProcessor.Processing;

public interface ILogEventNormalizer
{
    LogEvent Normalize(LogEvent? logEvent);
}
