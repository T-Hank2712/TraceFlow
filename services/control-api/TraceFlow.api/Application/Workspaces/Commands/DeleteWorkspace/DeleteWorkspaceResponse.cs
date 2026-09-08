namespace TraceFlow.Api.Application.Workspaces.Commands.DeleteWorkspace;

public record DeleteWorkspaceResponse(
    Ulid WorkspaceId,
    string DeleteMode,
    string Message);