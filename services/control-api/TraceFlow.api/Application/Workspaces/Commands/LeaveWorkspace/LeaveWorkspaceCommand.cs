using MediatR;

namespace TraceFlow.Api.Application.Workspaces.Commands.LeaveWorkspace;

public record LeaveWorkspaceCommand(
    Ulid WorkspaceId,
    Ulid UserId
) : IRequest<LeaveWorkspaceResponse>;