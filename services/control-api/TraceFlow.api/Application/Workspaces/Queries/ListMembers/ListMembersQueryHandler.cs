using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.AccessControl;
using TraceFlow.Api.Infrastructure.Persistence;
using TraceFlow.Api.Domain.Constants;

namespace TraceFlow.Api.Application.Workspaces.Queries.ListMembers;

public class ListMembersQueryHandler
    : IRequestHandler<ListMembersQuery, IReadOnlyList<WorkspaceMemberResponse>>
{
    private readonly AppDbContext _dbContext;
    private readonly WorkspaceAccessService _workspaceAccess;

    public ListMembersQueryHandler(
        AppDbContext dbContext,
        WorkspaceAccessService workspaceAccess)
    {
        _dbContext = dbContext;
        _workspaceAccess = workspaceAccess;
    }

    public async Task<IReadOnlyList<WorkspaceMemberResponse>> Handle(
        ListMembersQuery request,
        CancellationToken cancellationToken)
    {
        await _workspaceAccess.GetActiveMembershipAsync(
            request.WorkspaceId,
            request.UserId,
            "Workspace not found.",
            cancellationToken);

        return await _dbContext.WorkspaceMembers
            .AsNoTracking()
            .Where(member =>
                member.WorkspaceId == request.WorkspaceId &&
                member.Status == MembershipStatuses.Active)
            .OrderBy(member => member.Role == WorkspaceMemberRoles.Owner ? 0 :
                               member.Role == WorkspaceMemberRoles.Admin ? 1 : 2)
            .ThenBy(member => member.User.UserName)
            .Select(member => new WorkspaceMemberResponse(
                member.Id,
                member.UserId,
                member.User.UserName,
                member.User.Email,
                member.Role,
                member.Status,
                member.JoinedAt))
            .ToListAsync(cancellationToken);
    }
}
