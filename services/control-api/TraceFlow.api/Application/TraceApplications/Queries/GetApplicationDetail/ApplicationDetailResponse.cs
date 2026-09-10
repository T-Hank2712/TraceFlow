namespace TraceFlow.Api.Application.TraceApplications.Queries.GetApplicationDetail;

public record ApplicationDetailResponse(
    Ulid Id,
    Ulid WorkspaceId,
    Ulid ProjectId,
    string Name,
    string Slug,
    string? Description,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt);