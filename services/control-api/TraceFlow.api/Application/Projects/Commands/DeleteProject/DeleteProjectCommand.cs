using MediatR;

namespace TraceFlow.Api.Application.Projects.Commands.DeleteProject;

public record DeleteProjectCommand(
    Ulid WorkspaceId,
    Ulid ProjectId,
    Ulid UserId
) : IRequest<DeleteProjectResponse>;