namespace TraceFlow.Api.Application.Common.Security;

public interface IApiKeyRevocationStore
{
    Task RevokeAsync(Ulid apiKeyId, CancellationToken cancellationToken);
}
