namespace TraceFlow.Api.Application.Projects.Commands.LeaveProject;

public record LeaveProjectResponse(
    Ulid ProjectId,
    Ulid UserId,
    string Message);
