using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.AccessControl;
using TraceFlow.Api.Application.Common.Exceptions;
using TraceFlow.Api.Domain.Entities;
using TraceFlow.Api.Infrastructure.Persistence;
using TraceFlow.Api.Domain.Constants;
using TraceFlow.Api.Application.Common.Users;

namespace TraceFlow.Api.Application.Workspaces.Commands.InviteWorkspaceMember;

public class InviteWorkspaceMemberCommandHandler
    : IRequestHandler<InviteWorkspaceMemberCommand, InviteWorkspaceMemberResponse>
{
    private readonly AppDbContext _dbContext;
    private readonly WorkspaceAccessService _workspaceAccess;
    private readonly UserLookupService _userLookup;

    public InviteWorkspaceMemberCommandHandler(
        AppDbContext dbContext,
        WorkspaceAccessService workspaceAccess,
        UserLookupService userLookup)
    {
        _dbContext = dbContext;
        _workspaceAccess = workspaceAccess;
        _userLookup = userLookup;
    }

    public async Task<InviteWorkspaceMemberResponse> Handle(
        InviteWorkspaceMemberCommand request,
        CancellationToken cancellationToken)
    {
        var inviterMembership = await _workspaceAccess.GetActiveMembershipAsync(
            request.WorkspaceId,
            request.InvitedByUserId,
            "Workspace not found.",
            cancellationToken);

        _workspaceAccess.EnsureWorkspaceIsActive(
            inviterMembership.Workspace,
            "Archived workspace cannot be modified.");

        _workspaceAccess.EnsureWorkspaceManager(
            inviterMembership,
            "You do not have permission to invite workspace members.");

        var invitedUser = await _userLookup.GetActiveInviteTargetAsync(
            request.Identifier,
            request.InvitedByUserId,
            cancellationToken);

        var alreadyMember = await _dbContext.WorkspaceMembers
            .AnyAsync(
                member =>
                    member.WorkspaceId == request.WorkspaceId &&
                    member.UserId == invitedUser.Id &&
                    member.Status == MembershipStatuses.Active,
                cancellationToken);

        if (alreadyMember)
        {
            throw new ConflictException("User is already a workspace member.");
        }

        var alreadyInvited = await _dbContext.WorkspaceInvitations
            .AnyAsync(
                invitation =>
                    invitation.WorkspaceId == request.WorkspaceId &&
                    invitation.InvitedUserId == invitedUser.Id &&
                    invitation.Status == InvitationStatuses.Pending &&
                    invitation.ExpiresAt > DateTime.UtcNow,
                cancellationToken);

        if (alreadyInvited)
        {
            throw new ConflictException("User already has a pending invitation.");
        }

        var invitation = new WorkspaceInvitation(
            request.WorkspaceId,
            invitedUser.Id,
            request.InvitedByUserId,
            request.Role,
            DateTime.UtcNow.AddDays(InvitationDefaults.ExpiresAfterDays));

        _dbContext.WorkspaceInvitations.Add(invitation);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new InviteWorkspaceMemberResponse(
            invitation.Id,
            invitation.WorkspaceId,
            invitedUser.Id,
            invitedUser.UserName,
            invitedUser.Email,
            invitation.Role,
            invitation.Status,
            invitation.ExpiresAt);
    }
}
