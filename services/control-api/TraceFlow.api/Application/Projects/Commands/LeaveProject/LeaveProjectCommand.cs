using MediatR;

namespace TraceFlow.Api.Application.Projects.Commands.LeaveProject;

public record LeaveProjectCommand(
    Ulid WorkspaceId,
    Ulid ProjectId,
    Ulid UserId
) : IRequest<LeaveProjectResponse>;