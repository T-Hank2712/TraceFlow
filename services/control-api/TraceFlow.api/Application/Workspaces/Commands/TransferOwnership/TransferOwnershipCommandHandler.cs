using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.Exceptions;
using TraceFlow.Api.Domain.Constants;
using TraceFlow.Api.Infrastructure.Persistence;

namespace TraceFlow.Api.Application.Workspaces.Commands.TransferOwnership;

public class TransferOwnershipCommandHandler
    : IRequestHandler<TransferOwnershipCommand, TransferOwnershipResponse>
{
    private readonly AppDbContext _dbContext;

    public TransferOwnershipCommandHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TransferOwnershipResponse> Handle(
        TransferOwnershipCommand request,
        CancellationToken cancellationToken)
    {
        var actorMembership = await _dbContext.WorkspaceMembers
            .Include(member => member.Workspace)
            .FirstOrDefaultAsync(
                member =>
                    member.WorkspaceId == request.WorkspaceId &&
                    member.UserId == request.ActorUserId &&
                    member.Status == MembershipStatuses.Active,
                cancellationToken);

        if (actorMembership is null)
        {
            throw new NotFoundException("Workspace not found.");
        }

        if (actorMembership.Workspace.Status == ResourceStatuses.Archived)
        {
            throw new ConflictException("Archived workspace cannot be modified.");
        }

        if (actorMembership.Role != WorkspaceMemberRoles.Owner)
        {
            throw new ForbiddenException("Only workspace owner can transfer ownership.");
        }

        if (request.TargetUserId == request.ActorUserId)
        {
            throw new ConflictException("Target user is already the current owner.");
        }

        var targetMembership = await _dbContext.WorkspaceMembers
            .FirstOrDefaultAsync(
                member =>
                    member.WorkspaceId == request.WorkspaceId &&
                    member.UserId == request.TargetUserId &&
                    member.Status == MembershipStatuses.Active,
                cancellationToken);

        if (targetMembership is null)
        {
            throw new NotFoundException("Target workspace member not found.");
        }

        if (targetMembership.Role == WorkspaceMemberRoles.Owner)
        {
            throw new ConflictException("Target user is already a workspace owner.");
        }

        targetMembership.ChangeRole(WorkspaceMemberRoles.Owner);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new TransferOwnershipResponse(
            request.WorkspaceId,
            targetMembership.UserId,
            targetMembership.Role,
            targetMembership.UpdatedAt);
    }
}