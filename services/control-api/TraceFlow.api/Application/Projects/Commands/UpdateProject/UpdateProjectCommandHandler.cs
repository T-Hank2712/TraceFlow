using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.Exceptions;
using TraceFlow.Api.Domain.Constants;
using TraceFlow.Api.Infrastructure.Persistence;

namespace TraceFlow.Api.Application.Projects.Commands.UpdateProject;

public class UpdateProjectCommandHandler
    : IRequestHandler<UpdateProjectCommand, UpdateProjectResponse>
{
    private readonly AppDbContext _dbContext;

    public UpdateProjectCommandHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UpdateProjectResponse> Handle(
        UpdateProjectCommand request,
        CancellationToken cancellationToken)
    {
        var workspaceMembership = await _dbContext.WorkspaceMembers
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
            throw new ConflictException("Archived workspace cannot be modified.");
        }

        var project = await _dbContext.Projects
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

        var isProjectManager = await _dbContext.ProjectMembers
            .AnyAsync(
                member =>
                    member.ProjectId == request.ProjectId &&
                    member.UserId == request.UserId &&
                    member.Role == ProjectMemberRoles.Manager &&
                    member.Status == MembershipStatuses.Active,
                cancellationToken);

        if (!isWorkspaceManager && !isProjectManager)
        {
            throw new ForbiddenException("You do not have permission to update this project.");
        }

        if (!string.IsNullOrWhiteSpace(request.Slug))
        {
            var normalizedSlug = request.Slug.Trim().ToLowerInvariant();

            var slugExists = await _dbContext.Projects
                .AnyAsync(
                    existingProject =>
                        existingProject.WorkspaceId == request.WorkspaceId &&
                        existingProject.Slug == normalizedSlug &&
                        existingProject.Status == ResourceStatuses.Active &&
                        existingProject.Id != request.ProjectId,
                    cancellationToken);

            if (slugExists)
            {
                throw new ConflictException("Project slug is already taken.");
            }
        }

        project.UpdateProject(
            request.Name,
            request.Slug,
            request.Description);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateProjectResponse(
            project.Id,
            project.WorkspaceId,
            project.CreatedByUserId,
            project.Name,
            project.Slug,
            project.Description,
            project.Status,
            project.UpdatedAt);
    }
}