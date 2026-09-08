using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.Exceptions;
using TraceFlow.Api.Domain.Entities;
using TraceFlow.Api.Infrastructure.Persistence;
using TraceFlow.Api.Domain.Constants;

namespace TraceFlow.Api.Application.Workspaces.Commands.InviteWorkspaceMember;

public class InviteWorkspaceMemberCommandHandler
    : IRequestHandler<InviteWorkspaceMemberCommand, InviteWorkspaceMemberResponse>
{
    private readonly AppDbContext _dbContext;

    public InviteWorkspaceMemberCommandHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<InviteWorkspaceMemberResponse> Handle(
        InviteWorkspaceMemberCommand request,
        CancellationToken cancellationToken)
    {
        var inviterMembership = await _dbContext.WorkspaceMembers
            .Include(member => member.Workspace)
            .FirstOrDefaultAsync(
                member =>
                    member.WorkspaceId == request.WorkspaceId &&
                    member.UserId == request.InvitedByUserId,
                cancellationToken);

        if (inviterMembership is null)
        {
            throw new NotFoundException("Workspace not found.");
        }

        if (inviterMembership.Workspace.Status == ResourceStatuses.Archived)
        {
            throw new ConflictException("Archived workspace cannot be modified.");
        }

        if (inviterMembership.Role is not WorkspaceMemberRoles.Owner and not WorkspaceMemberRoles.Admin)
        {
            throw new ForbiddenException("You do not have permission to invite workspace members.");
        }

        var normalizedIdentifier = request.Identifier.Trim().ToLowerInvariant();

        var invitedUser = await _dbContext.Users
            .FirstOrDefaultAsync(
                user =>
                    user.Email.ToLower() == normalizedIdentifier ||
                    user.NormalizedUsername == normalizedIdentifier,
                cancellationToken);

        if (invitedUser is null)
        {
            throw new NotFoundException("User to invite not found.");
        }

        if (invitedUser.Status != UserStatuses.Active)
        {
            throw new ConflictException("Cannot invite inactive user.");
        }

        if (invitedUser.Id == request.InvitedByUserId)
        {
            throw new ConflictException("You cannot invite yourself.");
        }

        var alreadyMember = await _dbContext.WorkspaceMembers
            .AnyAsync(
                member =>
                    member.WorkspaceId == request.WorkspaceId &&
                    member.UserId == invitedUser.Id &&
                    member.Status == WorkspaceMemberStatuses.Active,
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
                    invitation.Status == WorkspaceInvitationStatuses.Pending &&
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
            DateTime.UtcNow.AddDays(7));

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