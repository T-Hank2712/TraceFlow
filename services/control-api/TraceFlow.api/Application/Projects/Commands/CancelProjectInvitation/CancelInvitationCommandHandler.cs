namespace TraceFlow.Api.Application.Projects.Commands.CancelProjectInvitation;

public class CancelProjectInvitationCommandHandler(
    AppDbContext dbContext,
    ProjectAccessService projectAccess,
    TimeProvider timeProvider
)
    : IRequestHandler<CancelProjectInvitationCommand, CancelProjectInvitationResponse>
{

    private readonly AppDbContext _dbContext = dbContext;

    private readonly ProjectAccessService _projectAccess = projectAccess;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<CancelProjectInvitationResponse> Handle(
           CancelProjectInvitationCommand request,
           CancellationToken cancellationToken)
    {
        var invitation = await _dbContext.ProjectInvitations
            .FirstOrDefaultAsync(
                invitation => invitation.Id == request.InvitationId,
                cancellationToken);

        if (invitation is null)
        {
            throw new NotFoundException("Project invitation not found.");
        }

        var access = await _projectAccess.GetProjectAccessAsync(
            invitation.ProjectId,
            request.ActorUserId,
            "Project invitation not found.",
            cancellationToken);

        _projectAccess.EnsureProjectManager(
            access,
            "You do not have permission to cancel project invitations.");

        if (invitation.Status != InvitationStatuses.Pending)
        {
            throw new ConflictException("Only pending invitation can be cancelled.");
        }

        invitation.Cancel(_timeProvider.GetUtcNow());

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CancelProjectInvitationResponse(
            invitation.Id,
            invitation.WorkspaceId,
            invitation.ProjectId,
            invitation.Status,
            invitation.UpdatedAt);
    }
}
