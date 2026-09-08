using MediatR;

namespace TraceFlow.Api.Application.Projects.Commands.CreateProject;

public record CreateProjectCommand(
    Ulid WorkspaceId,
    Ulid UserId,
    string Name,
    string Slug,
    string? Description
) : IRequest<CreateProjectResponse>;