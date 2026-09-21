namespace TraceFlow.Ingestion.Api.Contracts;

public sealed record Result<T>(
    bool Success,
    T? Data,
    ErrorResponse? Error,
    int StatusCode)
{
    public static Result<T> Ok(
        T data,
        int statusCode = StatusCodes.Status200OK)
        => new(true, data, null, statusCode);

    public static Result<T> Fail(
        string code,
        string message,
        int statusCode)
        => new(
            false,
            default,
            new ErrorResponse(code, message),
            statusCode);
}
