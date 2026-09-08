using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.Exceptions;
using TraceFlow.Api.Infrastructure.Persistence;
using TraceFlow.Api.Domain.Constants;

namespace TraceFlow.Api.Application.Workspaces.Commands.ChangeMemberRole;

public class ChangeMemberRoleCommandHandler
    : IRequestHandler<ChangeMemberRoleCommand, ChangeMemberRoleResponse>
{
    private readonly AppDbContext _dbContext;

    public ChangeMemberRoleCommandHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ChangeMemberRoleResponse> Handle(
        ChangeMemberRoleCommand request,
        CancellationToken cancellationToken)
    {
        var actorMembership = await _dbContext.WorkspaceMembers
            .Include(member => member.Workspace)
            .FirstOrDefaultAsync(
                member =>
                    member.WorkspaceId == request.WorkspaceId &&
                    member.UserId == request.ActorUserId &&
                    member.Status == WorkspaceMemberStatuses.Active,
                cancellationToken);

        if (actorMembership is null)
        {
            throw new NotFoundException("Workspace not found.");
        }

        if (actorMembership.Workspace.Status == WorkspaceStatuses.Archived)
        {
            throw new ConflictException("Archived workspace cannot be modified.");
        }

        if (actorMembership.Role is not WorkspaceMemberRoles.Owner and not WorkspaceMemberRoles.Admin)
        {
            throw new ForbiddenException("You do not have permission to change member roles.");
        }

        var targetMember = await _dbContext.WorkspaceMembers
            .FirstOrDefaultAsync(
                member =>
                    member.Id == request.MemberId &&
                    member.WorkspaceId == request.WorkspaceId &&
                    member.Status == WorkspaceMemberStatuses.Active,
                cancellationToken);

        if (targetMember is null)
        {
            throw new NotFoundException("Workspace member not found.");
        }

        var newRole = request.Role.Trim().ToLowerInvariant();

        if (targetMember.Role == WorkspaceMemberRoles.Owner && newRole != WorkspaceMemberRoles.Owner)
        {
            var ownerCount = await _dbContext.WorkspaceMembers
                .CountAsync(
                    member =>
                        member.WorkspaceId == request.WorkspaceId &&
                        member.Role == WorkspaceMemberRoles.Owner &&
                        member.Status == WorkspaceMemberStatuses.Active,
                    cancellationToken);

            if (ownerCount <= 1)
            {
                throw new ConflictException("Workspace must have at least one owner.");
            }
        }

        if (actorMembership.Role == WorkspaceMemberRoles.Admin && targetMember.Role == WorkspaceMemberRoles.Owner)
        {
            throw new ForbiddenException("Admin cannot change owner role.");
        }

        if (targetMember.Role == newRole)
        {
            throw new ConflictException("Member already has this role.");
        }

        targetMember.ChangeRole(newRole);

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