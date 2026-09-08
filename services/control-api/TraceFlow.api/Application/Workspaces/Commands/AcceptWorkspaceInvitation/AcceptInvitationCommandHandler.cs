using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.Exceptions;
using TraceFlow.Api.Domain.Entities;
using TraceFlow.Api.Infrastructure.Persistence;

namespace TraceFlow.Api.Application.Workspaces.Commands.AcceptWorkspaceInvitation;

public class AcceptInvitationCommandHandler
    : IRequestHandler<AcceptInvitationCommand, AcceptInvitationResponse>
{
    private readonly AppDbContext _dbContext;

    public AcceptInvitationCommandHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AcceptInvitationResponse> Handle(
        AcceptInvitationCommand request,
        CancellationToken cancellationToken)
    {
        var invitation = await _dbContext.WorkspaceInvitations
            .Include(invitation => invitation.Workspace)
            .FirstOrDefaultAsync(
                invitation =>
                    invitation.Id == request.InvitationId &&
                    invitation.InvitedUserId == request.UserId,
                cancellationToken);

        if (invitation is null)
        {
            throw new NotFoundException("Invitation not found.");
        }

        if (invitation.Workspace.Status == "archived")
        {
            throw new ConflictException("Archived workspace cannot be joined.");
        }

        var alreadyMember = await _dbContext.WorkspaceMembers
            .AnyAsync(
                member =>
                    member.WorkspaceId == invitation.WorkspaceId &&
                    member.UserId == request.UserId &&
                    member.Status == "active",
                cancellationToken);

        if (alreadyMember)
        {
            throw new ConflictException("User is already a workspace member.");
        }

        invitation.Accept();

        var member = new WorkspaceMember(
            invitation.WorkspaceId,
            request.UserId,
            invitation.Role);

        _dbContext.WorkspaceMembers.Add(member);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AcceptInvitationResponse(
            invitation.Id,
            invitation.WorkspaceId,
            member.Id,
            member.Role,
            invitation.Status);
    }
}