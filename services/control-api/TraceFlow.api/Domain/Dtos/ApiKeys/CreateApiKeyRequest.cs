// Domain/Dtos/ApiKeys/CreateApiKeyRequest.cs
namespace TraceFlow.Api.Domain.Dtos.ApiKeys;

public sealed record CreateApiKeyRequest(
    string Name,
    string Environment,
    int ExpirationPolicy);