using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.Exceptions;
using TraceFlow.Api.Domain.Constants;
using TraceFlow.Api.Infrastructure.Persistence;

namespace TraceFlow.Api.Application.Projects.Queries.ListProjectMembers;

public class ListProjectMembersQueryHandler
    : IRequestHandler<ListProjectMembersQuery, IReadOnlyList<ProjectMemberResponse>>
{
    private readonly AppDbContext _dbContext;

    public ListProjectMembersQueryHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ProjectMemberResponse>> Handle(
        ListProjectMembersQuery request,
        CancellationToken cancellationToken)
    {
        var workspaceMembership = await _dbContext.WorkspaceMembers
            .AsNoTracking()
            .Include(member => member.Workspace)
            .FirstOrDefaultAsync(
                member =>
                    member.WorkspaceId == request.WorkspaceId &&
                    member.UserId == request.UserId &&
                    member.Status == MembershipStatuses.Active,
                cancellationToken);

        if (workspaceMembership is null)
        {
            throw new NotFoundException("Project not found.");
        }

        if (workspaceMembership.Workspace.Status == ResourceStatuses.Archived)
        {
            throw new ConflictException("Archived workspace cannot be accessed.");
        }

        var projectExists = await _dbContext.Projects
            .AsNoTracking()
            .AnyAsync(
                project =>
                    project.Id == request.ProjectId &&
                    project.WorkspaceId == request.WorkspaceId &&
                    project.Status == ResourceStatuses.Active,
                cancellationToken);

        if (!projectExists)
        {
            throw new NotFoundException("Project not found.");
        }

        var isWorkspaceManager =
            workspaceMembership.Role is WorkspaceMemberRoles.Owner or WorkspaceMemberRoles.Admin;

        var hasProjectAccess = await _dbContext.ProjectMembers
            .AsNoTracking()
            .AnyAsync(
                member =>
                    member.ProjectId == request.ProjectId &&
                    member.UserId == request.UserId &&
                    member.Status == MembershipStatuses.Active,
                cancellationToken);

        if (!isWorkspaceManager && !hasProjectAccess)
        {
            throw new NotFoundException("Project not found.");
        }

        return await _dbContext.ProjectMembers
            .AsNoTracking()
            .Where(member =>
                member.ProjectId == request.ProjectId &&
                member.Status == MembershipStatuses.Active)
            .OrderBy(member => member.Role == ProjectMemberRoles.Manager ? 0 :
                               member.Role == ProjectMemberRoles.Developer ? 1 : 2)
            .ThenBy(member => member.User.UserName)
            .Select(member => new ProjectMemberResponse(
                member.Id,
                member.ProjectId,
                member.UserId,
                member.User.UserName,
                member.User.Email,
                member.Role,
                member.Status,
                member.JoinedAt))
            .ToListAsync(cancellationToken);
    }
}