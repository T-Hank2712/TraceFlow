using System.Collections.Concurrent;

namespace TraceFlow.Ingestion.Api.Security;

public sealed class ApiKeyValidationFlightStore
{
    private readonly ConcurrentDictionary<string, Lazy<Task<ApiKeyValidationResult>>> _flights = new();

    public async Task<ApiKeyValidationResult> RunAsync(
        string apiKeyFingerprint,
        Func<Task<ApiKeyValidationResult>> operation,
        CancellationToken waitCancellationToken)
    {
        var flight = _flights.GetOrAdd(
            apiKeyFingerprint,
            _ => CreateFlight(
                apiKeyFingerprint,
                operation));

        return await flight.Value.WaitAsync(waitCancellationToken);
    }

    private Lazy<Task<ApiKeyValidationResult>> CreateFlight(
        string apiKeyFingerprint,
        Func<Task<ApiKeyValidationResult>> operation)
    {
        Lazy<Task<ApiKeyValidationResult>>? flight = null;

        flight = new Lazy<Task<ApiKeyValidationResult>>(
            async () =>
            {
                try
                {
                    return await operation();
                }
                finally
                {
                    if (flight is not null)
                    {
                        _flights.TryRemove(
                            new KeyValuePair<string, Lazy<Task<ApiKeyValidationResult>>>(
                                apiKeyFingerprint,
                                flight));
                    }
                }
            },
            LazyThreadSafetyMode.ExecutionAndPublication);

        return flight;
    }
}
