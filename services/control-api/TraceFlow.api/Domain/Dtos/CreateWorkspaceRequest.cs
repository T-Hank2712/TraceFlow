namespace TraceFlow.Api.Domain.Dtos;

public record CreateWorkspaceRequest(
    string Name,
    string Slug,
    string? Description
);