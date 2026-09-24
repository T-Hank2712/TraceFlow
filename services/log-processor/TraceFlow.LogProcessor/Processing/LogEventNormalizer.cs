using System.Text.Json;
using TraceFlow.LogProcessor.Contracts;

namespace TraceFlow.LogProcessor.Processing;

public sealed class LogEventNormalizer : ILogEventNormalizer
{
    public LogEvent Normalize(LogEvent? logEvent)
    {
        if (logEvent is null)
        {
            throw new ArgumentNullException(
                nameof(logEvent),
                "LogEvent payload cannot be null.");
        }

        return logEvent with
        {
            EventId = logEvent.EventId,
            Service = logEvent.Service.Trim(),
            Environment = logEvent.Environment.Trim().ToLowerInvariant(),
            Message = logEvent.Message.Trim(),
            Timestamp = logEvent.Timestamp.ToUniversalTime(),
            Metadata = NormalizeMetadata(logEvent.Metadata)
        };
    }

    private static Dictionary<string, object?>? NormalizeMetadata(
        Dictionary<string, object?>? metadata)
    {
        if (metadata is null)
        {
            return null;
        }

        return metadata.ToDictionary(
            pair => pair.Key,
            pair => NormalizeValue(pair.Value));
    }

    private static object? NormalizeValue(object? value)
    {
        if (value is not JsonElement element)
        {
            return value;
        }

        return NormalizeJsonElement(element);
    }

    private static object? NormalizeJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Null =>
                null,

            JsonValueKind.String =>
                element.GetString(),

            JsonValueKind.Number =>
                element.TryGetInt64(out var longValue)
                    ? longValue
                    : element.GetDouble(),

            JsonValueKind.True =>
                true,

            JsonValueKind.False =>
                false,

            JsonValueKind.Object =>
                element.EnumerateObject()
                    .ToDictionary(
                        property => property.Name,
                        property => NormalizeJsonElement(property.Value)),

            JsonValueKind.Array =>
                element.EnumerateArray()
                    .Select(NormalizeJsonElement)
                    .ToList(),

            _ => null
        };
    }
}