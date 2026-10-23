namespace TraceFlow.Api.Application.Projects.Commands.ChangeProjectMemberRole;

public class ChangeProjectMemberRoleCommandHandler(
        AppDbContext dbContext,
        ProjectAccessService projectAccess,
        TimeProvider timeProvider)
    : IRequestHandler<ChangeProjectMemberRoleCommand, ChangeProjectMemberRoleResponse>
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly ProjectAccessService _projectAccess = projectAccess;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<ChangeProjectMemberRoleResponse> Handle(
           ChangeProjectMemberRoleCommand request,
           CancellationToken cancellationToken)
    {
        var targetMember = await _dbContext.ProjectMembers
            .FirstOrDefaultAsync(
                member =>
                    member.Id == request.MemberId &&
                    member.Status == MembershipStatuses.Active,
                cancellationToken);

        if (targetMember is null)
        {
            throw new NotFoundException("Project member not found.");
        }

        var access = await _projectAccess.GetProjectAccessAsync(
            targetMember.ProjectId,
            request.ActorUserId,
            "Project not found.",
            cancellationToken);

        _projectAccess.EnsureProjectManager(
            access,
            "You do not have permission to change project member roles.");

        if (targetMember.UserId == request.ActorUserId)
        {
            throw new ConflictException(
                "You cannot change your own project role.");
        }

        var newRole = request.Role.Trim().ToLowerInvariant();

        if (targetMember.Role == newRole)
        {
            throw new ConflictException("Project member already has this role.");
        }

        if (targetMember.Role == ProjectMemberRoles.Manager &&
            newRole != ProjectMemberRoles.Manager)
        {
            var managerCount = await _dbContext.ProjectMembers
                .CountAsync(
                    member =>
                        member.ProjectId == targetMember.ProjectId &&
                        member.Role == ProjectMemberRoles.Manager &&
                        member.Status == MembershipStatuses.Active,
                    cancellationToken);

            if (managerCount <= 1)
            {
                throw new ConflictException("Project must have at least one manager.");
            }
        }

        targetMember.ChangeRole(newRole, _timeProvider.GetUtcNow());

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ChangeProjectMemberRoleResponse(
            targetMember.Id,
            targetMember.ProjectId,
            targetMember.UserId,
            targetMember.Role,
            targetMember.Status,
            targetMember.UpdatedAt);
    }
}
