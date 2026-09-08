using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.Exceptions;
using TraceFlow.Api.Infrastructure.Persistence;
using TraceFlow.Api.Domain.Constants;

namespace TraceFlow.Api.Application.Workspaces.Queries.ListMembers;

public class ListMembersQueryHandler
    : IRequestHandler<ListMembersQuery, IReadOnlyList<WorkspaceMemberResponse>>
{
    private readonly AppDbContext _dbContext;

    public ListMembersQueryHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<WorkspaceMemberResponse>> Handle(
        ListMembersQuery request,
        CancellationToken cancellationToken)
    {
        var hasAccess = await _dbContext.WorkspaceMembers
            .AsNoTracking()
            .AnyAsync(
                member =>
                    member.WorkspaceId == request.WorkspaceId &&
                    member.UserId == request.UserId &&
                    member.Status == MembershipStatuses.Active,
                cancellationToken);

        if (!hasAccess)
        {
            throw new NotFoundException("Workspace not found.");
        }

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