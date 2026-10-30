namespace TraceFlow.Api.Application.Projects.Commands.LeaveProject;

public record LeaveProjectCommand(
    Ulid ProjectId,
    Ulid UserId
) : IRequest<LeaveProjectResponse>;
