namespace TraceFlow.Ingestion.Api.Contracts;

public static class ValidationErrorMapper
{
    public static IReadOnlyDictionary<string, string[]> ToDetails(
        IEnumerable<FluentValidation.Results.ValidationFailure> errors)
    {
        return errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => ToCamelCase(group.Key),
                group => group
                    .Select(error => error.ErrorMessage)
                    .Distinct()
                    .ToArray());
    }

    private static string ToCamelCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "request";
        }

        return char.ToLowerInvariant(value[0]) + value[1..];
    }
}
