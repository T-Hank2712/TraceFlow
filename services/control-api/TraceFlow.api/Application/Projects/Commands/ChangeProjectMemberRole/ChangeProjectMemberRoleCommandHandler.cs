using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.AccessControl;
using TraceFlow.Api.Application.Common.Exceptions;
using TraceFlow.Api.Domain.Constants;
using TraceFlow.Api.Infrastructure.Persistence;

namespace TraceFlow.Api.Application.Projects.Commands.ChangeProjectMemberRole;

public class ChangeProjectMemberRoleCommandHandler
    : IRequestHandler<ChangeProjectMemberRoleCommand, ChangeProjectMemberRoleResponse>
{
    private readonly AppDbContext _dbContext;
    private readonly ProjectAccessService _projectAccess;

    public ChangeProjectMemberRoleCommandHandler(
        AppDbContext dbContext,
        ProjectAccessService projectAccess)
    {
        _dbContext = dbContext;
        _projectAccess = projectAccess;
    }

    public async Task<ChangeProjectMemberRoleResponse> Handle(
        ChangeProjectMemberRoleCommand request,
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
            "You do not have permission to change project member roles.");

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

        var newRole = request.Role.Trim().ToLowerInvariant();

        if (targetMember.Role == newRole)
        {
            throw new ConflictException("Project member already has this role.");
        }

        if (targetMember.Role == ProjectMemberRoles.Manager &&
            newRole != ProjectMemberRoles.Manager)
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

        targetMember.ChangeRole(newRole);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ChangeProjectMemberRoleResponse(
            targetMember.Id,
            targetMember.ProjectId,
            targetMember.UserId,
            targetMember.Role,
            targetMember.Status,
            targetMember.UpdatedAt);
    }
}
