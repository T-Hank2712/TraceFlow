using TraceFlow.Ingestion.Api.Clients;
using TraceFlow.Ingestion.Api.Contracts.Authentication;

namespace TraceFlow.Ingestion.Api.IntegrationTests.Infrastructure;

public sealed class FakeApiKeyValidator : IApiKeyValidator
{
    private readonly Dictionary<string, ApiKeyValidationResult> _results = new();

    public int CallCount { get; private set; }

    public void Register(string apiKey, ApiKeyValidationResult result)
    {
        _results[apiKey] = result;
    }

    public Task<ApiKeyValidationResult> ValidateAsync(
        string apiKey,
        CancellationToken cancellationToken)
    {
        CallCount++;

        return Task.FromResult(
            _results.TryGetValue(apiKey, out var result)
                ? result
                : new ApiKeyValidationResult(false, null, null, null, null));
    }
}