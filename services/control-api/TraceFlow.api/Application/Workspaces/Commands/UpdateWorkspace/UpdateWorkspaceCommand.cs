using MediatR;
namespace TraceFlow.Api.Application.Workspaces.Commands.UpdateWorkspace;
public record UpdateWorkspaceCommand(
    Ulid WorkspaceId,
    Ulid UserId,
    string? Name,
    string? Slug,
    string? Description
) : IRequest<UpdateWorkspaceResponse>;