using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.Exceptions;
using TraceFlow.Api.Domain.Constants;
using TraceFlow.Api.Infrastructure.Persistence;

namespace TraceFlow.Api.Application.Projects.Commands.ChangeProjectMemberRole;

public class ChangeProjectMemberRoleCommandHandler
    : IRequestHandler<ChangeProjectMemberRoleCommand, ChangeProjectMemberRoleResponse>
{
    private readonly AppDbContext _dbContext;

    public ChangeProjectMemberRoleCommandHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ChangeProjectMemberRoleResponse> Handle(
        ChangeProjectMemberRoleCommand request,
        CancellationToken cancellationToken)
    {
        var workspaceMembership = await _dbContext.WorkspaceMembers
            .Include(member => member.Workspace)
            .FirstOrDefaultAsync(
                member =>
                    member.WorkspaceId == request.WorkspaceId &&
                    member.UserId == request.ActorUserId &&
                    member.Status == MembershipStatuses.Active,
                cancellationToken);

        if (workspaceMembership is null)
        {
            throw new NotFoundException("Project not found.");
        }

        if (workspaceMembership.Workspace.Status == ResourceStatuses.Archived)
        {
            throw new ConflictException("Archived workspace cannot be modified.");
        }

        var project = await _dbContext.Projects
            .FirstOrDefaultAsync(
                project =>
                    project.Id == request.ProjectId &&
                    project.WorkspaceId == request.WorkspaceId &&
                    project.Status == ResourceStatuses.Active,
                cancellationToken);

        if (project is null)
        {
            throw new NotFoundException("Project not found.");
        }

        var actorProjectMembership = await _dbContext.ProjectMembers
            .FirstOrDefaultAsync(
                member =>
                    member.ProjectId == request.ProjectId &&
                    member.UserId == request.ActorUserId &&
                    member.Status == MembershipStatuses.Active,
                cancellationToken);

        var isWorkspaceManager =
            workspaceMembership.Role is WorkspaceMemberRoles.Owner or WorkspaceMemberRoles.Admin;

        var isProjectManager =
            actorProjectMembership?.Role == ProjectMemberRoles.Manager;

        if (!isWorkspaceManager && !isProjectManager)
        {
            throw new ForbiddenException("You do not have permission to change project member roles.");
        }

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