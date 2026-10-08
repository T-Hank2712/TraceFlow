namespace TraceFlow.Api.Application.Workspaces.Commands.RemoveMember;

public class RemoveMemberCommandHandler(
        AppDbContext dbContext,
        WorkspaceAccessService workspaceAccess,
        TimeProvider timeProvider)
    : IRequestHandler<RemoveMemberCommand, RemoveMemberResponse>
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly WorkspaceAccessService _workspaceAccess = workspaceAccess;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<RemoveMemberResponse> Handle(
        RemoveMemberCommand request,
        CancellationToken cancellationToken)
    {
        var strategy = _dbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction =
                await _dbContext.Database.BeginTransactionAsync(cancellationToken);

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
                "You do not have permission to remove workspace members.");

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

            var isTargetPrimaryOwner = targetMember.UserId == actorMembership.Workspace.OwnerUserId;

            if (isTargetPrimaryOwner)
            {
                throw new ConflictException(
                    "Workspace primary owner cannot be removed.");
            }

            if (targetMember.Role == WorkspaceMemberRoles.Owner &&
                actorMembership.Role != WorkspaceMemberRoles.Owner)
            {
                throw new ForbiddenException(
                    "Only workspace owner can remove another workspace owner.");
            }

            var utcNow = _timeProvider.GetUtcNow();

            var projectMemberships = await _dbContext.ProjectMembers
                .Where(projectMember =>
                    projectMember.UserId == targetMember.UserId &&
                    projectMember.Project.WorkspaceId == request.WorkspaceId)
                .ToListAsync(cancellationToken);

            foreach (var projectMembership in projectMemberships)
            {
                projectMembership.Remove(utcNow);
            }

            targetMember.Remove(utcNow);

            await _dbContext.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return new RemoveMemberResponse(
                targetMember.Id,
                targetMember.WorkspaceId,
                targetMember.UserId);
        });
    }
}