using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.Exceptions;
using TraceFlow.Api.Domain.Constants;
using TraceFlow.Api.Infrastructure.Persistence;

namespace TraceFlow.Api.Application.TraceApplications.Queries.ListTraceApplications;

public class ListTraceApplicationsQueryHandler
    : IRequestHandler<ListTraceApplicationsQuery, IReadOnlyList<TraceApplicationSummaryResponse>>
{
    private readonly AppDbContext _dbContext;

    public ListTraceApplicationsQueryHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<TraceApplicationSummaryResponse>> Handle(
        ListTraceApplicationsQuery request,
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

        var project = await _dbContext.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(
                project =>
                    project.Id == request.ProjectId &&
                    project.WorkspaceId == request.WorkspaceId &&
                    project.Status == ResourceStatuses.Active,
                cancellationToken);

        if (project is null)
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

        return await _dbContext.TraceApplications
            .AsNoTracking()
            .Where(application =>
                application.ProjectId == request.ProjectId &&
                application.Status == ResourceStatuses.Active)
            .OrderByDescending(application => application.CreatedAt)
            .Select(application => new TraceApplicationSummaryResponse(
                application.Id,
                application.ProjectId,
                application.Name,
                application.Slug,
                application.Description,
                application.Status,
                application.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}