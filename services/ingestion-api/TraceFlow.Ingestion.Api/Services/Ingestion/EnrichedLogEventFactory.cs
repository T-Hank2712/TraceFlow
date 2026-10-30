namespace TraceFlow.Ingestion.Api.Services.Ingestion;

public sealed class EnrichedLogEventFactory(TimeProvider timeProvider)
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

        var utcNow = timeProvider.GetUtcNow();

        return new EnrichedLogEvent(
            EventId: EventId,
            BatchID: BatchId,
            WorkspaceId: workspaceId,
            ProjectId: projectId,
            ApplicationId: applicationId,
            Environment: tenant.Environment,
            Timestamp: request.Timestamp ?? utcNow,
            Level: request.Level,
            Service: request.Service.Trim(),
            Message: request.Message.Trim(),
            TraceId: request.TraceId,
            CorrelationId: request.CorrelationId,
            Metadata: request.Metadata,
            ReceivedAt: utcNow
        );
    }
}
