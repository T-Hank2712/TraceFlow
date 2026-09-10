using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.AccessControl;
using TraceFlow.Api.Application.Common.Exceptions;
using TraceFlow.Api.Domain.Constants;
using TraceFlow.Api.Infrastructure.Persistence;

namespace TraceFlow.Api.Application.Projects.Commands.CancelProjectInvitation;

public class CancelProjectInvitationCommandHandler
    : IRequestHandler<CancelProjectInvitationCommand, CancelProjectInvitationResponse>
{
    private readonly AppDbContext _dbContext;
    private readonly ProjectAccessService _projectAccess;

    public CancelProjectInvitationCommandHandler(
        AppDbContext dbContext,
        ProjectAccessService projectAccess)
    {
        _dbContext = dbContext;
        _projectAccess = projectAccess;
    }

    public async Task<CancelProjectInvitationResponse> Handle(
        CancelProjectInvitationCommand request,
        CancellationToken cancellationToken)
    {
        var access = await _projectAccess.GetProjectAccessAsync(
            request.WorkspaceId,
            request.ProjectId,
            request.ActorUserId,
            "Project invitation not found.",
            cancellationToken);

        _projectAccess.EnsureProjectManager(
            access,
            "You do not have permission to cancel project invitations.");

        var invitation = await _dbContext.ProjectInvitations
            .FirstOrDefaultAsync(
                invitation =>
                    invitation.Id == request.InvitationId &&
                    invitation.WorkspaceId == request.WorkspaceId &&
                    invitation.ProjectId == request.ProjectId,
                cancellationToken);

        if (invitation is null)
        {
            throw new NotFoundException("Project invitation not found.");
        }

        if (invitation.Status != InvitationStatuses.Pending)
        {
            throw new ConflictException("Only pending invitation can be cancelled.");
        }

        invitation.Cancel();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CancelProjectInvitationResponse(
            invitation.Id,
            invitation.WorkspaceId,
            invitation.ProjectId,
            invitation.Status,
            invitation.UpdatedAt);
    }
}
