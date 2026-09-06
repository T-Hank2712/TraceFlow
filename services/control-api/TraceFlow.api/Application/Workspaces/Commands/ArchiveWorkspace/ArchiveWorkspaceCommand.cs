using MediatR;

namespace TraceFlow.Api.Application.Workspaces.Commands.ArchiveWorkspace;

public record ArchiveWorkspaceCommand(
    Ulid WorkspaceId,
    Ulid UserId
) : IRequest<ArchiveWorkspaceResponse>;