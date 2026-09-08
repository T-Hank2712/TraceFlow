using MediatR;

namespace TraceFlow.Api.Application.Workspaces.Commands.DeleteWorkspace;

public record DeleteWorkspaceCommand(
    Ulid WorkspaceId,
    Ulid UserId
) : IRequest<DeleteWorkspaceResponse>;