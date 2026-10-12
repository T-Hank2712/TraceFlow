namespace TraceFlow.Api.Application.Workspaces.Queries.GetWorkspaceById;

public record WorkspaceDetailResponse(
    Ulid Id,
    string Name,
    string Slug,
    string? Description,
    string Status,
    string CurrentUserRole,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
