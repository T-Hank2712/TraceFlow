namespace TraceFlow.Api.Application.Workspaces.Commands.LeaveWorkspace;

public record LeaveWorkspaceResponse(
    Ulid WorkspaceId,
    Ulid UserId,
    string Message);