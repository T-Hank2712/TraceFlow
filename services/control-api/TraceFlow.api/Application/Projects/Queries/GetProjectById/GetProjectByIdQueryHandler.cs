using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.Exceptions;
using TraceFlow.Api.Domain.Constants;
using TraceFlow.Api.Infrastructure.Persistence;

namespace TraceFlow.Api.Application.Projects.Queries.GetProjectById;

public class GetProjectByIdQueryHandler
    : IRequestHandler<GetProjectByIdQuery, ProjectDetailResponse>
{
    private readonly AppDbContext _dbContext;

    public GetProjectByIdQueryHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ProjectDetailResponse> Handle(
        GetProjectByIdQuery request,
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
            throw new NotFoundException("Project not found.");
        }

        if (membership.Workspace.Status == ResourceStatuses.Archived)
        {
            throw new ConflictException("Archived workspace cannot be accessed.");
        }

        var canAccessAllProjects =
            membership.Role is WorkspaceMemberRoles.Owner or WorkspaceMemberRoles.Admin;

        var projectQuery = _dbContext.Projects
            .AsNoTracking()
            .Where(project =>
                project.Id == request.ProjectId &&
                project.WorkspaceId == request.WorkspaceId &&
                project.Status == ResourceStatuses.Active);

        if (!canAccessAllProjects)
        {
            projectQuery = projectQuery.Where(project =>
                project.Members.Any(member =>
                    member.UserId == request.UserId &&
                    member.Status == MembershipStatuses.Active));
        }

        var project = await projectQuery
            .Select(project => new ProjectDetailResponse(
                project.Id,
                project.WorkspaceId,
                project.CreatedByUserId,
                project.CreatedByUser.UserName,
                project.Name,
                project.Slug,
                project.Description,
                project.Status,
                project.CreatedAt,
                project.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        if (project is null)
        {
            throw new NotFoundException("Project not found.");
        }

        return project;
    }
}