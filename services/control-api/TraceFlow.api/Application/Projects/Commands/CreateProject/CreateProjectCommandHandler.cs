using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.AccessControl;
using TraceFlow.Api.Application.Common.Exceptions;
using TraceFlow.Api.Domain.Constants;
using TraceFlow.Api.Domain.Entities;
using TraceFlow.Api.Infrastructure.Persistence;

namespace TraceFlow.Api.Application.Projects.Commands.CreateProject;

public class CreateProjectCommandHandler
    : IRequestHandler<CreateProjectCommand, CreateProjectResponse>
{
    private readonly AppDbContext _dbContext;
    private readonly WorkspaceAccessService _workspaceAccess;

    public CreateProjectCommandHandler(
        AppDbContext dbContext,
        WorkspaceAccessService workspaceAccess)
    {
        _dbContext = dbContext;
        _workspaceAccess = workspaceAccess;
    }

    public async Task<CreateProjectResponse> Handle(
        CreateProjectCommand request,
        CancellationToken cancellationToken)
    {
        var membership = await _workspaceAccess.GetActiveMembershipAsync(
            request.WorkspaceId,
            request.UserId,
            "Workspace not found.",
            cancellationToken);

        _workspaceAccess.EnsureWorkspaceIsActive(
            membership.Workspace,
            "Archived workspace cannot be modified.");

        _workspaceAccess.EnsureWorkspaceManager(
            membership,
            "You do not have permission to create projects.");

        var normalizedSlug = request.Slug.Trim().ToLowerInvariant();

        var slugExists = await _dbContext.Projects
            .AnyAsync(
                project =>
                    project.WorkspaceId == request.WorkspaceId &&
                    project.Slug == normalizedSlug &&
                    project.Status == ResourceStatuses.Active,
                cancellationToken);

        if (slugExists)
        {
            throw new ConflictException("Project slug is already taken.");
        }

        var project = new Project(
            request.WorkspaceId,
            request.UserId,
            request.Name,
            request.Slug,
            request.Description);

        _dbContext.Projects.Add(project);

        var managerUserIds = await _dbContext.WorkspaceMembers
            .AsNoTracking()
            .Where(member =>
                member.WorkspaceId == request.WorkspaceId &&
                member.Status == MembershipStatuses.Active &&
                (member.Role == WorkspaceMemberRoles.Owner ||
                member.Role == WorkspaceMemberRoles.Admin))
            .Select(member => member.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var projectMembers = managerUserIds
            .Select(userId => new ProjectMember(
                project.Id,
                userId,
                ProjectMemberRoles.Manager))
            .ToList();

        _dbContext.ProjectMembers.AddRange(projectMembers);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateProjectResponse(
            project.Id,
            project.WorkspaceId,
            project.CreatedByUserId,
            project.Name,
            project.Slug,
            project.Description,
            project.Status,
            project.CreatedAt);
    }
}
