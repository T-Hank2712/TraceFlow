using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.Exceptions;
using TraceFlow.Api.Infrastructure.Persistence;

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
                    member.Status == "active",
                cancellationToken);

        if (!hasAccess)
        {
            throw new NotFoundException("Workspace not found.");
        }

        return await _dbContext.WorkspaceMembers
            .AsNoTracking()
            .Where(member =>
                member.WorkspaceId == request.WorkspaceId &&
                member.Status == "active")
            .OrderBy(member => member.Role == "owner" ? 0 :
                               member.Role == "admin" ? 1 : 2)
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