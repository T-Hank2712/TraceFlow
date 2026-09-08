using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.Exceptions;
using TraceFlow.Api.Infrastructure.Persistence;

namespace TraceFlow.Api.Application.Workspaces.Queries.InvitationSent;

public class InvitationSentQueryHandler
    : IRequestHandler<InvitationSentQuery, IReadOnlyList<InvitationSentResponse>>
{
    private readonly AppDbContext _dbContext;

    public InvitationSentQueryHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<InvitationSentResponse>> Handle(
        InvitationSentQuery request,
        CancellationToken cancellationToken)
    {
        var membership = await _dbContext.WorkspaceMembers
            .AsNoTracking()
            .Include(member => member.Workspace)
            .FirstOrDefaultAsync(
                member =>
                    member.WorkspaceId == request.WorkspaceId &&
                    member.UserId == request.UserId,
                cancellationToken);

        if (membership is null)
        {
            throw new NotFoundException("Workspace not found.");
        }

        if (membership.Workspace.Status == "archived")
        {
            throw new ConflictException("Archived workspace cannot be accessed.");
        }

        if (membership.Role is not "owner" and not "admin")
        {
            throw new ForbiddenException("You do not have permission to view workspace invitations.");
        }

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