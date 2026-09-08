using MediatR;

namespace TraceFlow.Api.Application.Projects.Queries.GetProjectById;

public record GetProjectByIdQuery(
    Ulid WorkspaceId,
    Ulid ProjectId,
    Ulid UserId
) : IRequest<ProjectDetailResponse>;