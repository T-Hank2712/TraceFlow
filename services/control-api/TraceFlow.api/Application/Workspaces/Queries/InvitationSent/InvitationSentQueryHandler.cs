using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.AccessControl;
using TraceFlow.Api.Infrastructure.Persistence;

namespace TraceFlow.Api.Application.Workspaces.Queries.InvitationSent;

public class InvitationSentQueryHandler
    : IRequestHandler<InvitationSentQuery, IReadOnlyList<InvitationSentResponse>>
{
    private readonly AppDbContext _dbContext;
    private readonly WorkspaceAccessService _workspaceAccess;

    public InvitationSentQueryHandler(
        AppDbContext dbContext,
        WorkspaceAccessService workspaceAccess)
    {
        _dbContext = dbContext;
        _workspaceAccess = workspaceAccess;
    }

    public async Task<IReadOnlyList<InvitationSentResponse>> Handle(
        InvitationSentQuery request,
        CancellationToken cancellationToken)
    {
        var membership = await _workspaceAccess.GetActiveMembershipAsync(
            request.WorkspaceId,
            request.UserId,
            "Workspace not found.",
            cancellationToken);

        _workspaceAccess.EnsureWorkspaceIsActive(
            membership.Workspace,
            "Archived workspace cannot be accessed.");

        _workspaceAccess.EnsureWorkspaceManager(
            membership,
            "You do not have permission to view workspace invitations.");

        return await _dbContext.WorkspaceInvitations
            .AsNoTracking()
            .Where(invitation => invitation.WorkspaceId == request.WorkspaceId)
            .OrderByDescending(invitation => invitation.CreatedAt)
            .Select(invitation => new InvitationSentResponse(
                invitation.Id,
                invitation.WorkspaceId,
                invitation.InvitedUserId,
                invitation.InvitedUser.UserName,
                invitation.InvitedUser.Email,
                invitation.InvitedByUserId,
                invitation.InvitedByUser.UserName,
                invitation.Role,
                invitation.Status,
                invitation.ExpiresAt,
                invitation.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
