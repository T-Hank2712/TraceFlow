namespace TraceFlow.Api.Application.Workspaces.Commands.TransferOwnership;

public class TransferOwnershipCommandHandler(
    AppDbContext dbContext,
    TimeProvider timeProvider)
    : IRequestHandler<TransferOwnershipCommand, TransferOwnershipResponse>
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<TransferOwnershipResponse> Handle(
        TransferOwnershipCommand request,
        CancellationToken cancellationToken)
    {
        if (request.TargetUserId == request.ActorUserId)
        {
            throw new ConflictException(
                "Target user is already the current owner.");
        }

        var strategy = _dbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction =
                await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            var members = await _dbContext.WorkspaceMembers
                .Include(member => member.Workspace)
                .Where(member =>
                    member.WorkspaceId == request.WorkspaceId &&
                    (member.UserId == request.ActorUserId ||
                     member.UserId == request.TargetUserId) &&
                    member.Status == MembershipStatuses.Active)
                .ToListAsync(cancellationToken);

            var actorMembership = members.FirstOrDefault(
                member => member.UserId == request.ActorUserId);

            if (actorMembership is null)
            {
                throw new NotFoundException("Workspace not found.");
            }

            if (actorMembership.Workspace.Status == ResourceStatuses.Archived)
            {
                throw new ConflictException(
                    "Archived workspace cannot be modified.");
            }

            if (actorMembership.Role != WorkspaceMemberRoles.Owner)
            {
                throw new ForbiddenException(
                    "Only workspace owner can transfer ownership.");
            }

            if (actorMembership.Workspace.OwnerUserId != request.ActorUserId)
            {
                throw new ConflictException(
                    "Workspace ownership state is inconsistent.");
            }

            var targetMembership = members.FirstOrDefault(
                member => member.UserId == request.TargetUserId);

            if (targetMembership is null)
            {
                throw new NotFoundException(
                    "Target workspace member not found.");
            }

            if (targetMembership.Role == WorkspaceMemberRoles.Owner)
            {
                throw new ConflictException(
                    "Target user is already a workspace owner.");
            }

            var utcNow = _timeProvider.GetUtcNow();

            actorMembership.Workspace.TransferOwnership(
                request.TargetUserId,
                utcNow);

            targetMembership.ChangeRole(
                WorkspaceMemberRoles.Owner,
                utcNow);

            actorMembership.ChangeRole(
                WorkspaceMemberRoles.Admin,
                utcNow);

            await _dbContext.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return new TransferOwnershipResponse(
                request.WorkspaceId,
                targetMembership.UserId,
                targetMembership.Role,
                targetMembership.UpdatedAt);
        });
    }
}