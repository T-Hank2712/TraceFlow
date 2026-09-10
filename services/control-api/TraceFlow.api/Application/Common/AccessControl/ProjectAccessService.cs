using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.Exceptions;
using TraceFlow.Api.Domain.Constants;
using TraceFlow.Api.Domain.Entities;
using TraceFlow.Api.Infrastructure.Persistence;

namespace TraceFlow.Api.Application.Common.AccessControl;

public class ProjectAccessService
{
    private readonly AppDbContext _dbContext;
    private readonly WorkspaceAccessService _workspaceAccess;

    public ProjectAccessService(
        AppDbContext dbContext,
        WorkspaceAccessService workspaceAccessService)
    {
        _dbContext = dbContext;
        _workspaceAccess = workspaceAccessService;
    }

    public async Task<ProjectAccessContext> GetProjectAccessAsync(
        Ulid workspaceId,
        Ulid projectId,
        Ulid userId,
        string notFoundMessage,
        CancellationToken cancellationToken,
        string archivedWorkspaceMessage = "Archived workspace cannot be modified.")
    {
        var workspaceMembership = await _workspaceAccess.GetActiveMembershipAsync(
            workspaceId,
            userId,
            notFoundMessage,
            cancellationToken);

        _workspaceAccess.EnsureWorkspaceIsActive(
            workspaceMembership.Workspace,
            archivedWorkspaceMessage);

        var project = await _dbContext.Projects
            .FirstOrDefaultAsync(
                project =>
                    project.Id == projectId &&
                    project.WorkspaceId == workspaceId &&
                    project.Status == ResourceStatuses.Active,
                cancellationToken);

        if (project is null)
        {
            throw new NotFoundException(notFoundMessage);
        }

        var projectMembership = await _dbContext.ProjectMembers
            .AsNoTracking()
            .FirstOrDefaultAsync(
                member =>
                    member.ProjectId == projectId &&
                    member.UserId == userId &&
                    member.Status == MembershipStatuses.Active,
                cancellationToken);

        return new ProjectAccessContext(
            workspaceMembership,
            project,
            projectMembership);
    }

    public void EnsureProjectManager(
        ProjectAccessContext access,
        string forbiddenMessage)
    {
        if (!access.IsWorkspaceManager && !access.IsProjectManager)
        {
            throw new ForbiddenException(forbiddenMessage);
        }
    }

    public void EnsureProjectDeveloperOrManager(
        ProjectAccessContext access,
        string forbiddenMessage)
    {
        if (!access.IsWorkspaceManager &&
            access.ProjectMembership?.Role is not ProjectMemberRoles.Manager and not ProjectMemberRoles.Developer)
        {
            throw new ForbiddenException(forbiddenMessage);
        }
    }

    public void EnsureProjectMember(
        ProjectAccessContext access,
        string forbiddenMessage)
    {
        if (!access.IsWorkspaceManager && access.ProjectMembership is null)
        {
            throw new ForbiddenException(forbiddenMessage);
        }
    }
}

public sealed record ProjectAccessContext(
    WorkspaceMember WorkspaceMembership,
    Project Project,
    ProjectMember? ProjectMembership)
{
    public bool IsWorkspaceManager => WorkspaceAccessService.IsWorkspaceManager(WorkspaceMembership);

    public bool IsProjectManager => ProjectMembership?.Role == ProjectMemberRoles.Manager;
}
