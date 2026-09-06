using MediatR;

namespace TraceFlow.Api.Application.Workspaces.Queries.GetWorkspaceById;

public record GetWorkspaceByIdQuery(
    Ulid WorkspaceId,
    Ulid UserId
) : IRequest<WorkspaceDetailResponse>;