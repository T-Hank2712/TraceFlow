namespace TraceFlow.Api.Application.Projects.Commands.DeleteProject;

public record DeleteProjectCommand(
    Ulid ProjectId,
    Ulid UserId
) : IRequest<DeleteProjectResponse>;
