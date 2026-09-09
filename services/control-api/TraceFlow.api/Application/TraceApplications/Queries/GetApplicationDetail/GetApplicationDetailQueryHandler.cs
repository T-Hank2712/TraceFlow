using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.Exceptions;
using TraceFlow.Api.Domain.Constants;
using TraceFlow.Api.Infrastructure.Persistence;

namespace TraceFlow.Api.Application.TraceApplications.Queries.GetApplicationDetail;

public class GetApplicationDetailQueryHandler
    : IRequestHandler<GetApplicationDetailQuery, ApplicationDetailResponse>
{
    private readonly AppDbContext _dbContext;

    public GetApplicationDetailQueryHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ApplicationDetailResponse> Handle(
        GetApplicationDetailQuery request,
        CancellationToken cancellationToken)
    {
        var workspaceMember = await _dbContext.WorkspaceMembers
            .FirstOrDefaultAsync(x =>
                x.WorkspaceId == request.WorkspaceId &&
                x.UserId == request.UserId &&
                x.Status == MembershipStatuses.Active,
                cancellationToken);

        if (workspaceMember is null)
        {
            throw new NotFoundException("Workspace not found.");
        }

        var project = await _dbContext.Projects
            .FirstOrDefaultAsync(x =>
                x.Id == request.ProjectId &&
                x.WorkspaceId == request.WorkspaceId &&
                x.Status == ResourceStatuses.Active,
                cancellationToken);

        if (project is null)
        {
            throw new NotFoundException("Project not found.");
        }

        var isWorkspaceOwnerOrAdmin =
            workspaceMember.Role == WorkspaceMemberRoles.Owner ||
            workspaceMember.Role == WorkspaceMemberRoles.Admin;

        var isProjectMember = await _dbContext.ProjectMembers
            .AnyAsync(x =>
                x.ProjectId == request.ProjectId &&
                x.UserId == request.UserId &&
                x.Status == MembershipStatuses.Active,
                cancellationToken);

        if (!isWorkspaceOwnerOrAdmin && !isProjectMember)
        {
            throw new NotFoundException("Project not found.");
        }

        var application = await _dbContext.TraceApplications
            .FirstOrDefaultAsync(x =>
                x.Id == request.ApplicationId &&
                x.ProjectId == request.ProjectId &&
                x.Status == ResourceStatuses.Active,
                cancellationToken);

        if (application is null)
        {
            throw new NotFoundException("Application not found.");
        }

        return new ApplicationDetailResponse(
            application.Id,
            request.WorkspaceId,
            application.ProjectId,
            application.Name,
            application.Slug,
            application.Description,
            application.Status,
            application.CreatedAt,
            application.UpdatedAt);
    }
}