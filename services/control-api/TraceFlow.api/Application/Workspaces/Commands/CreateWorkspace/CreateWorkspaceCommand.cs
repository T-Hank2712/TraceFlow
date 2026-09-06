using MediatR;

namespace TraceFlow.Api.Application.Workspaces.Commands.CreateWorkspace;

public record CreateWorkspaceCommand(
    Ulid UserId,
    string Name,
    string Slug,
    string? Description
) : IRequest<CreateWorkspaceResponse>;