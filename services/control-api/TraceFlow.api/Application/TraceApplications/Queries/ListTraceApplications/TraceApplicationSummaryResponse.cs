namespace TraceFlow.Api.Application.TraceApplications.Queries.ListTraceApplications;

public record TraceApplicationSummaryResponse(
    Ulid Id,
    Ulid ProjectId,
    string Name,
    string Slug,
    string? Description,
    string Status,
    DateTime CreatedAt);