namespace TraceFlow.Api.Application.Projects.Commands.DeleteProject;

public record DeleteProjectResponse(
    Ulid ProjectId,
    Ulid WorkspaceId,
    string DeleteMode,
    string Message);