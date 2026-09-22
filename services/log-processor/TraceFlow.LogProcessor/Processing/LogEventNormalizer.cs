using TraceFlow.LogProcessor.Contracts;
using System;

namespace TraceFlow.LogProcessor.Processing;

public sealed class LogEventNormalizer : ILogEventNormalizer
{
    public LogEvent Normalize(LogEvent? logEvent)
    {
        if (logEvent is null)
        {
            throw new ArgumentNullException(nameof(logEvent), "LogEvent payload cannot be null.");
        }
        return logEvent with
        {
            EventId = logEvent.EventId,
            Service = logEvent.Service.Trim(),
            Environment = logEvent.Environment.Trim().ToLowerInvariant(),
            Message = logEvent.Message.Trim(),
            Timestamp = logEvent.Timestamp.ToUniversalTime()
        };
    }
}