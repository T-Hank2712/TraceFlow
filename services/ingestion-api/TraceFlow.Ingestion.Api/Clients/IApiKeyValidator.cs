namespace TraceFlow.Ingestion.Api.Clients;

public interface IApiKeyValidator
{
    Task<ApiKeyValidationResult> ValidateAsync(string apiKey, CancellationToken cancellationToken);
}
