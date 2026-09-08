using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.Exceptions;
using TraceFlow.Api.Domain.Constants;
using TraceFlow.Api.Infrastructure.Persistence;

namespace TraceFlow.Api.Application.Projects.Queries.ListProjects;

public class ListProjectsQueryHandler
    : IRequestHandler<ListProjectsQuery, IReadOnlyList<ProjectSummaryResponse>>
{
    private readonly AppDbContext _dbContext;

    public ListProjectsQueryHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ProjectSummaryResponse>> Handle(
        ListProjectsQuery request,
        CancellationToken cancellationToken)
    {
        var membership = await _dbContext.WorkspaceMembers
            .AsNoTracking()
            .Include(member => member.Workspace)
            .FirstOrDefaultAsync(
                member =>
                    member.WorkspaceId == request.WorkspaceId &&
                    member.UserId == request.UserId &&
                    member.Status == MembershipStatuses.Active,
                cancellationToken);

        if (membership is null)
        {
            throw new NotFoundException("Workspace not found.");
        }

        if (membership.Workspace.Status == ResourceStatuses.Archived)
        {
            throw new ConflictException("Archived workspace cannot be accessed.");
        }

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