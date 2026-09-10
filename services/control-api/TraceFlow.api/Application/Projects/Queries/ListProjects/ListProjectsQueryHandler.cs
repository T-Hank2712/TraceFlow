using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.AccessControl;
using TraceFlow.Api.Application.Common.Exceptions;
using TraceFlow.Api.Domain.Constants;
using TraceFlow.Api.Infrastructure.Persistence;

namespace TraceFlow.Api.Application.Projects.Queries.ListProjects;

public class ListProjectsQueryHandler
    : IRequestHandler<ListProjectsQuery, IReadOnlyList<ProjectSummaryResponse>>
{
    private readonly AppDbContext _dbContext;
    private readonly WorkspaceAccessService _workspaceAccess;

    public ListProjectsQueryHandler(
        AppDbContext dbContext,
        WorkspaceAccessService workspaceAccess)
    {
        _dbContext = dbContext;
        _workspaceAccess = workspaceAccess;
    }

    public async Task<IReadOnlyList<ProjectSummaryResponse>> Handle(
        ListProjectsQuery request,
        CancellationToken cancellationToken)
    {
        var membership = await _workspaceAccess.GetActiveMembershipAsync(
            request.WorkspaceId,
            request.UserId,
            "Workspace not found.",
            cancellationToken);

        _workspaceAccess.EnsureWorkspaceIsActive(
            membership.Workspace,
            "Archived workspace cannot be accessed.");

        var projectsQuery = _dbContext.Projects
            .AsNoTracking()
            .Where(project =>
                project.WorkspaceId == request.WorkspaceId &&
                project.Status == ResourceStatuses.Active);

        if (membership.Role is not WorkspaceMemberRoles.Owner and not WorkspaceMemberRoles.Admin)
        {
            projectsQuery = projectsQuery
                .Where(project =>
                    project.Members.Any(member =>
                        member.UserId == request.UserId &&
                        member.Status == MembershipStatuses.Active));
        }

        return await projectsQuery
            .OrderByDescending(project => project.CreatedAt)
            .Select(project => new ProjectSummaryResponse(
                project.Id,
                project.WorkspaceId,
                project.Name,
                project.Slug,
                project.Description,
                project.Status,
                project.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
