namespace TraceFlow.Api.Application.Projects.Commands.RemoveProjectMember;

public class RemoveProjectMemberCommandHandler(
        AppDbContext dbContext,
        ProjectAccessService projectAccess,
        TimeProvider timeProvider)
    : IRequestHandler<RemoveProjectMemberCommand, RemoveProjectMemberResponse>
{

    private readonly AppDbContext _dbContext = dbContext;
    private readonly ProjectAccessService _projectAccess = projectAccess;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<RemoveProjectMemberResponse> Handle(
           RemoveProjectMemberCommand request,
           CancellationToken cancellationToken)
    {
        var access = await _projectAccess.GetProjectAccessAsync(
            request.WorkspaceId,
            request.ProjectId,
            request.ActorUserId,
            "Project not found.",
            cancellationToken);

        _projectAccess.EnsureProjectManager(
            access,
            "You do not have permission to remove project members.");

        var targetMember = await _dbContext.ProjectMembers
            .FirstOrDefaultAsync(
                member =>
                    member.Id == request.MemberId &&
                    member.ProjectId == request.ProjectId &&
                    member.Status == MembershipStatuses.Active,
                cancellationToken);

        if (targetMember is null)
        {
            throw new NotFoundException("Project member not found.");
        }

        if (targetMember.Role == ProjectMemberRoles.Manager)
        {
            var managerCount = await _dbContext.ProjectMembers
                .CountAsync(
                    member =>
                        member.ProjectId == request.ProjectId &&
                        member.Role == ProjectMemberRoles.Manager &&
                        member.Status == MembershipStatuses.Active,
                    cancellationToken);

            if (managerCount <= 1)
            {
                throw new ConflictException("Project must have at least one manager.");
            }
        }

        var response = new RemoveProjectMemberResponse(
            targetMember.Id,
            targetMember.ProjectId,
            targetMember.UserId);

        targetMember.Remove(_timeProvider.GetUtcNow());

        await _dbContext.SaveChangesAsync(cancellationToken);

        return response;
    }
}
