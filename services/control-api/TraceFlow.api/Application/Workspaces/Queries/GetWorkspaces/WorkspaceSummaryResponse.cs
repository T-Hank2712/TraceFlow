namespace TraceFlow.Api.Application.Workspaces.Queries.GetWorkspaces;
public record WorkspaceSummaryResponse(
    Ulid WorkspacesId,
    string Name,
    string Slug,
    string? Description,
    string Role,
    string Status,
    DateTime CreatedAt
);