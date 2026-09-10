using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.AccessControl;
using TraceFlow.Api.Application.Common.Exceptions;
using TraceFlow.Api.Domain.Constants;
using TraceFlow.Api.Domain.Entities;
using TraceFlow.Api.Infrastructure.Persistence;

namespace TraceFlow.Api.Application.Projects.Commands.InviteProjectMember;

public class InviteProjectMemberCommandHandler
    : IRequestHandler<InviteProjectMemberCommand, InviteProjectMemberResponse>
{
    private readonly AppDbContext _dbContext;
    private readonly ProjectAccessService _projectAccess;

    public InviteProjectMemberCommandHandler(
        AppDbContext dbContext,
        ProjectAccessService projectAccess)
    {
        _dbContext = dbContext;
        _projectAccess = projectAccess;
    }

    public async Task<InviteProjectMemberResponse> Handle(
        InviteProjectMemberCommand request,
        CancellationToken cancellationToken)
    {
        var access = await _projectAccess.GetProjectAccessAsync(
            request.WorkspaceId,
            request.ProjectId,
            request.InvitedByUserId,
            "Project not found.",
            cancellationToken);

        _projectAccess.EnsureProjectManager(
            access,
            "You do not have permission to invite project members.");

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

        var alreadyProjectMember = await _dbContext.ProjectMembers
            .AnyAsync(
                member =>
                    member.ProjectId == request.ProjectId &&
                    member.UserId == invitedUser.Id &&
                    member.Status == MembershipStatuses.Active,
                cancellationToken);

        if (alreadyProjectMember)
        {
            throw new ConflictException("User is already a project member.");
        }

        var alreadyInvited = await _dbContext.ProjectInvitations
            .AnyAsync(
                invitation =>
                    invitation.ProjectId == request.ProjectId &&
                    invitation.InvitedUserId == invitedUser.Id &&
                    invitation.Status == InvitationStatuses.Pending &&
                    invitation.ExpiresAt > DateTime.UtcNow,
                cancellationToken);

        if (alreadyInvited)
        {
            throw new ConflictException("User already has a pending project invitation.");
        }

        var invitation = new ProjectInvitation(
            request.WorkspaceId,
            request.ProjectId,
            invitedUser.Id,
            request.InvitedByUserId,
            request.Role,
            DateTime.UtcNow.AddDays(7));

        _dbContext.ProjectInvitations.Add(invitation);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new InviteProjectMemberResponse(
            invitation.Id,
            invitation.WorkspaceId,
            invitation.ProjectId,
            invitedUser.Id,
            invitedUser.UserName,
            invitedUser.Email,
            invitation.Role,
            invitation.Status,
            invitation.ExpiresAt);
    }
}
