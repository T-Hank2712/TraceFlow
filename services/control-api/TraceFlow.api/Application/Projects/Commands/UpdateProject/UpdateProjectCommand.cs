namespace TraceFlow.Api.Application.Projects.Commands.UpdateProject;

public record UpdateProjectCommand(
    Ulid ProjectId,
    Ulid UserId,
    string? Name,
    string? Slug,
    string? Description
) : IRequest<UpdateProjectResponse>;
