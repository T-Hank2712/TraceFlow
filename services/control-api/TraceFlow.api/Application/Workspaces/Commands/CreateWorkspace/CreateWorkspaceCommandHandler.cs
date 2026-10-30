namespace TraceFlow.Api.Application.Workspaces.Commands.CreateWorkspace;

public class CreateWorkspaceCommandHandler(
    AppDbContext dbContext,
    TimeProvider timeProvider)
    : IRequestHandler<CreateWorkspaceCommand, CreateWorkspaceResponse>
{

    private readonly AppDbContext _dbContext = dbContext;
    private readonly TimeProvider _timeProvider = timeProvider;


    public async Task<CreateWorkspaceResponse> Handle(
        CreateWorkspaceCommand request,
        CancellationToken cancellationToken)
    {
        var normalizedSlug = request.Slug.Trim().ToLower();

        var slugExists = await _dbContext.Workspaces
            .AnyAsync(
                workspace =>
                    workspace.OwnerUserId == request.UserId &&
                    workspace.Slug == normalizedSlug &&
                    workspace.Status == ResourceStatuses.Active,
                cancellationToken);

        if (slugExists)
        {
            throw new ConflictException(
                "Workspace slug is already taken.");
        }

        var utcNow = _timeProvider.GetUtcNow();

        var workspace = new Workspace(
            request.UserId,
            request.Name,
            request.Slug,
            request.Description,
            utcNow);

        _dbContext.Workspaces.Add(workspace);

        var ownerMember = new WorkspaceMember(
            workspace.Id,
            request.UserId,
            WorkspaceMemberRoles.Owner,
            utcNow);

        _dbContext.WorkspaceMembers.Add(ownerMember);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateWorkspaceResponse(
            workspace.Id,
            workspace.Name,
            workspace.Slug,
            workspace.Description,
            workspace.Status,
            ownerMember.Role);
    }
}
