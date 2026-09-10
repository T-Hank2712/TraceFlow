using MediatR;
using TraceFlow.Api.Application.Common.AccessControl;

namespace TraceFlow.Api.Application.Workspaces.Queries.GetWorkspaceById;

public class GetWorkspaceByIdQueryHandler
    : IRequestHandler<GetWorkspaceByIdQuery, WorkspaceDetailResponse>
{
    private readonly WorkspaceAccessService _workspaceAccess;

    public GetWorkspaceByIdQueryHandler(WorkspaceAccessService workspaceAccess)
    {
        _workspaceAccess = workspaceAccess;
    }

    public async Task<WorkspaceDetailResponse> Handle(
        GetWorkspaceByIdQuery request,
        CancellationToken cancellationToken)
    {
        var membership = await _workspaceAccess.GetActiveMembershipAsync(
            request.WorkspaceId,
            request.UserId,
            "Workspace not found.",
            cancellationToken);

        return new WorkspaceDetailResponse(
            membership.Workspace.Id,
            membership.Workspace.Name,
            membership.Workspace.Slug,
            membership.Workspace.Description,
            membership.Workspace.Status,
            membership.Role,
            membership.Workspace.CreatedAt,
            membership.Workspace.UpdatedAt
        );
    }
}
