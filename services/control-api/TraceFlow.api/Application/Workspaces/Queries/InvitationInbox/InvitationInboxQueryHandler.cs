using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Infrastructure.Persistence;
using TraceFlow.Api.Domain.Constants;

namespace TraceFlow.Api.Application.Workspaces.Queries.InvitationInbox;

public class InvitationInboxQueryHandler
    : IRequestHandler<InvitationInboxQuery, IReadOnlyList<InvitationInboxResponse>>
{
    private readonly AppDbContext _dbContext;

    public InvitationInboxQueryHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<InvitationInboxResponse>> Handle(
        InvitationInboxQuery request,
        CancellationToken cancellationToken)
    {
        return await _dbContext.WorkspaceInvitations
            .AsNoTracking()
            .Where(invitation =>
                invitation.InvitedUserId == request.UserId &&
                invitation.Status == InvitationStatuses.Pending &&
                invitation.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(invitation => invitation.CreatedAt)
            .Select(invitation => new InvitationInboxResponse(
                invitation.Id,
                invitation.WorkspaceId,
                invitation.Workspace.Name,
                invitation.Workspace.Slug,
                invitation.InvitedByUserId,
                invitation.InvitedByUser.UserName,
                invitation.Role,
                invitation.Status,
                invitation.ExpiresAt,
                invitation.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}