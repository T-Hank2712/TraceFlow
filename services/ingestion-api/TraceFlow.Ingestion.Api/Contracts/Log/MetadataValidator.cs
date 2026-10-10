namespace TraceFlow.Ingestion.Api.Contracts.Log;

public static class MetadataValidator
{
    public static bool IsValid(
        Dictionary<string, object?>? metadata,
        IngestionOptions options)
    {
        if (metadata is null)
        {
            return true;
        }

        if (metadata.Count > options.MaxMetadataKeys)
        {
            return false;
        }

        return ValidateObject(
            metadata,
            options,
            depth: 1);
    }

    private static bool ValidateObject(
        IReadOnlyDictionary<string, object?> metadata,
        IngestionOptions options,
        int depth)
    {
        if (depth > options.MaxMetadataDepth)
        {
            return false;
        }

        if (metadata.Count > options.MaxMetadataKeys)
        {
            return false;
        }

        foreach (var pair in metadata)
        {
            if (string.IsNullOrWhiteSpace(pair.Key) ||
                pair.Key.Length > options.MaxMetadataKeyLength)
            {
                return false;
            }

            if (!ValidateValue(
                    pair.Value,
                    options,
                    depth))
            {
                return false;
            }
        }

        return true;
    }

    private static bool ValidateValue(
        object? value,
        IngestionOptions options,
        int depth)
    {
        if (value is null)
        {
            return true;
        }

        if (value is string stringValue)
        {
            return stringValue.Length <= options.MaxMetadataStringValueLength;
        }

        if (value is JsonElement jsonElement)
        {
            return ValidateJsonElement(
                jsonElement,
                options,
                depth);
        }

        return true;
    }

    private static bool ValidateJsonElement(
        JsonElement element,
        IngestionOptions options,
        int depth)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Null => true,
            JsonValueKind.True => true,
            JsonValueKind.False => true,
            JsonValueKind.Number => true,

            JsonValueKind.String =>
                (element.GetString()?.Length ?? 0)
                    <= options.MaxMetadataStringValueLength,

            JsonValueKind.Object =>
                ValidateJsonObject(
                    element,
                    options,
                    depth + 1),

            JsonValueKind.Array =>
                ValidateJsonArray(
                    element,
                    options,
                    depth + 1),

            _ => false
        };
    }

    private static bool ValidateJsonObject(
        JsonElement element,
        IngestionOptions options,
        int depth)
    {
        if (depth > options.MaxMetadataDepth)
        {
            return false;
        }

        var count = 0;

        foreach (var property in element.EnumerateObject())
        {
            count++;

            if (count > options.MaxMetadataKeys)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(property.Name) ||
                property.Name.Length > options.MaxMetadataKeyLength)
            {
                return false;
            }

            if (!ValidateJsonElement(
                    property.Value,
                    options,
                    depth))
            {
                return false;
            }
        }

        return true;
    }

    private static bool ValidateJsonArray(
        JsonElement element,
        IngestionOptions options,
        int depth)
    {
        if (depth > options.MaxMetadataDepth)
        {
            return false;
        }

        var count = 0;

        foreach (var item in element.EnumerateArray())
        {
            count++;

            if (count > options.MaxMetadataArrayLength)
            {
                return false;
            }

            if (!ValidateJsonElement(
                    item,
                    options,
                    depth))
            {
                return false;
            }
        }

        return true;
    }
}
