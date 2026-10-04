namespace TraceFlow.Api.Application.Workspaces.Commands.UpdateWorkspace;

public class UpdateWorkspaceCommandHandler(
        AppDbContext dbContext,
        WorkspaceAccessService workspaceAccess,
        TimeProvider timeProvider)
    : IRequestHandler<UpdateWorkspaceCommand, UpdateWorkspaceResponse>
{

    private readonly AppDbContext _dbContext = dbContext;

    private readonly WorkspaceAccessService _workspaceAccess = workspaceAccess;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<UpdateWorkspaceResponse> Handle(
           UpdateWorkspaceCommand request,
           CancellationToken cancellationToken)
    {
        var membership = await _workspaceAccess.GetActiveMembershipAsync(
            request.WorkspaceId,
            request.UserId,
            "Workspace not found.",
            cancellationToken);

        _workspaceAccess.EnsureWorkspaceManager(
            membership,
            "You do not have permission to update this workspace.");

        _workspaceAccess.EnsureWorkspaceIsActive(
            membership.Workspace,
            "Archived workspace cannot be updated.");

        if (!string.IsNullOrWhiteSpace(request.Slug))
        {
            var normalizedSlug = request.Slug.Trim().ToLowerInvariant();

            var slugExists = await _dbContext.Workspaces
                .AnyAsync(
                    workspace =>
                        workspace.OwnerUserId == membership.Workspace.OwnerUserId &&
                        workspace.Slug == normalizedSlug &&
                        workspace.Status == ResourceStatuses.Active &&
                        workspace.Id != request.WorkspaceId,
                    cancellationToken);

            if (slugExists)
            {
                throw new ConflictException("Workspace slug is already taken.");
            }
        }

        membership.Workspace.UpdateWorkspace(
            request.Name,
            request.Slug,
            request.Description,
            _timeProvider.GetUtcNow());

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateWorkspaceResponse(
            membership.Workspace.Id,
            membership.Workspace.Name,
            membership.Workspace.Slug,
            membership.Workspace.Description,
            membership.Workspace.Status,
            membership.Workspace.CreatedAt,
            membership.Workspace.UpdatedAt);
    }
}
