namespace TraceFlow.Api.Application.Workspaces.Commands.ChangeMemberRole;

public class ChangeMemberRoleCommandHandler(
        AppDbContext dbContext,
        WorkspaceAccessService workspaceAccess,
        TimeProvider timeProvider)
    : IRequestHandler<ChangeMemberRoleCommand, ChangeMemberRoleResponse>
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly WorkspaceAccessService _workspaceAccess = workspaceAccess;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<ChangeMemberRoleResponse> Handle(
        ChangeMemberRoleCommand request,
        CancellationToken cancellationToken)
    {
        var actorMembership = await _workspaceAccess.GetActiveMembershipAsync(
            request.WorkspaceId,
            request.ActorUserId,
            "Workspace not found.",
            cancellationToken);

        _workspaceAccess.EnsureWorkspaceIsActive(
            actorMembership.Workspace,
            "Archived workspace cannot be modified.");

        _workspaceAccess.EnsureWorkspaceManager(
            actorMembership,
            "You do not have permission to change member roles.");

        var targetMember = await _dbContext.WorkspaceMembers
            .FirstOrDefaultAsync(
                member =>
                    member.Id == request.MemberId &&
                    member.WorkspaceId == request.WorkspaceId &&
                    member.Status == MembershipStatuses.Active,
                cancellationToken);

        if (targetMember is null)
        {
            throw new NotFoundException("Workspace member not found.");
        }

        var newRole = request.Role.Trim().ToLowerInvariant();

        if (targetMember.Role == newRole)
        {
            throw new ConflictException("Member already has this role.");
        }

        var isTargetPrimaryOwner =
            targetMember.UserId == actorMembership.Workspace.OwnerUserId;

        if (isTargetPrimaryOwner && newRole != WorkspaceMemberRoles.Owner)
        {
            throw new ConflictException(
                "Workspace primary owner role cannot be changed.");
        }

        if (targetMember.Role == WorkspaceMemberRoles.Owner &&
            actorMembership.Role != WorkspaceMemberRoles.Owner)
        {
            throw new ForbiddenException(
                "Only workspace owner can change another workspace owner's role.");
        }

        if (newRole == WorkspaceMemberRoles.Owner &&
            actorMembership.Role != WorkspaceMemberRoles.Owner)
        {
            throw new ForbiddenException(
                "Only workspace owner can promote members to owner.");
        }

        var utcNow = _timeProvider.GetUtcNow();

        targetMember.ChangeRole(newRole, utcNow);

        if (newRole == WorkspaceMemberRoles.Admin)
        {
            var activeProjectIds = await _dbContext.Projects
                .Where(project =>
                    project.WorkspaceId == request.WorkspaceId &&
                    project.Status == ResourceStatuses.Active)
                .Select(project => project.Id)
                .ToListAsync(cancellationToken);

            var existingProjectMembers = await _dbContext.ProjectMembers
                .Where(member =>
                    member.UserId == targetMember.UserId &&
                    member.Project.WorkspaceId == request.WorkspaceId &&
                    member.Project.Status == ResourceStatuses.Active)
                .ToListAsync(cancellationToken);

            foreach (var existingProjectMember in existingProjectMembers)
            {
                existingProjectMember.ChangeRole(ProjectMemberRoles.Manager, utcNow);
                existingProjectMember.Activate(utcNow);
            }

            var existingProjectIds = existingProjectMembers
                .Select(member => member.ProjectId)
                .ToHashSet();

            var missingProjectMembers = activeProjectIds
                .Where(projectId => !existingProjectIds.Contains(projectId))
                .Select(projectId => new ProjectMember(
                    projectId,
                    targetMember.UserId,
                    ProjectMemberRoles.Manager,
                    utcNow))
                .ToList();

            _dbContext.ProjectMembers.AddRange(missingProjectMembers);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ChangeMemberRoleResponse(
            targetMember.Id,
            targetMember.UserId,
            targetMember.WorkspaceId,
            targetMember.Role,
            targetMember.Status,
            targetMember.UpdatedAt);
    }
}