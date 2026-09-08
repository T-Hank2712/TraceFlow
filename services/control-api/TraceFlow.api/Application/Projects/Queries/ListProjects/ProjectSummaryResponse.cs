namespace TraceFlow.Api.Application.Projects.Queries.ListProjects;
public record ProjectSummaryResponse(
    Ulid Id,
    Ulid WorkspaceId,
    string Name,
    string Slug,
    string? Description,
    string Status,
    DateTime CreatedAt
);