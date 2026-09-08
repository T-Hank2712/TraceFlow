namespace TraceFlow.Api.Application.Projects.Queries.GetProjectById;

public record ProjectDetailResponse(
    Ulid Id,
    Ulid WorkspaceId,
    Ulid CreatedByUserId,
    string CreatedByUserName,
    string Name,
    string Slug,
    string? Description,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt);