using TraceFlow.Ingestion.Api.Clients;
using TraceFlow.Ingestion.Api.Contracts.Log;

namespace TraceFlow.Ingestion.Api.Ingestion;

public sealed class EnrichedLogEventFactory
{
    public EnrichedLogEvent Create(IngestLogRequest request, ApiKeyValidationResult tenant, Ulid EventId, Ulid BatchId)
    {
        if (tenant.WorkspaceId is not Ulid workspaceId ||
            tenant.ProjectId is not Ulid projectId ||
            tenant.ApplicationId is not Ulid applicationId ||
            tenant.Environment is not string)
        {
            throw new InvalidOperationException(
                "Tenant context is missing required resource identifiers.");
        }

        return new EnrichedLogEvent(
            EventId: Ulid.NewUlid(),
            BatchID: Ulid.NewUlid(),
            WorkspaceId: workspaceId,
            ProjectId: projectId,
            ApplicationId: applicationId,
            Environment: tenant.Environment,
            Timestamp: request.Timestamp ?? DateTimeOffset.UtcNow,
            Level: request.Level,
            Service: request.Service.Trim(),
            Message: request.Message.Trim(),
            TraceId: request.TraceId,
            CorrelationId: request.CorrelationId,
            Metadata: request.Metadata,
            ReceivedAt: DateTimeOffset.UtcNow
        );
    }
}
