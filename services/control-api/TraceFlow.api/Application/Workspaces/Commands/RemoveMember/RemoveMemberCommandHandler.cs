using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.AccessControl;
using TraceFlow.Api.Application.Common.Exceptions;
using TraceFlow.Api.Domain.Constants;
using TraceFlow.Api.Infrastructure.Persistence;

namespace TraceFlow.Api.Application.Workspaces.Commands.RemoveMember;

public class RemoveMemberCommandHandler
    : IRequestHandler<RemoveMemberCommand, RemoveMemberResponse>
{
    private readonly AppDbContext _dbContext;
    private readonly WorkspaceAccessService _workspaceAccess;

    public RemoveMemberCommandHandler(
        AppDbContext dbContext,
        WorkspaceAccessService workspaceAccess)
    {
        _dbContext = dbContext;
        _workspaceAccess = workspaceAccess;
    }

    public async Task<RemoveMemberResponse> Handle(
        RemoveMemberCommand request,
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

        if (actorMembership.Role == WorkspaceMemberRoles.Admin &&
            targetMember.Role == WorkspaceMemberRoles.Owner)
        {
            throw new ForbiddenException("Admin cannot remove workspace owner.");
        }

        if (targetMember.Role == WorkspaceMemberRoles.Owner)
        {
            var ownerCount = await _dbContext.WorkspaceMembers
                .CountAsync(
                    member =>
                        member.WorkspaceId == request.WorkspaceId &&
                        member.Role == WorkspaceMemberRoles.Owner &&
                        member.Status == MembershipStatuses.Active,
                    cancellationToken);

            if (ownerCount <= 1)
            {
                throw new ConflictException("Workspace must have at least one owner.");
            }
        }

        var projectMembers = await _dbContext.ProjectMembers
            .Where(projectMember =>
                projectMember.UserId == targetMember.UserId &&
                projectMember.Project.WorkspaceId == request.WorkspaceId)
            .ToListAsync(cancellationToken);

        _dbContext.ProjectMembers.RemoveRange(projectMembers);

        var response = new RemoveMemberResponse(
            targetMember.Id,
            targetMember.WorkspaceId,
            targetMember.UserId);

        _dbContext.WorkspaceMembers.Remove(targetMember);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return response;
    }
}
