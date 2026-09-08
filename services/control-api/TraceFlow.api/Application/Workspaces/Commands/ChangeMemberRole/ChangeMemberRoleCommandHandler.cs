using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.Exceptions;
using TraceFlow.Api.Infrastructure.Persistence;
using TraceFlow.Api.Domain.Constants;
using TraceFlow.Api.Domain.Entities;

namespace TraceFlow.Api.Application.Workspaces.Commands.ChangeMemberRole;

public class ChangeMemberRoleCommandHandler
    : IRequestHandler<ChangeMemberRoleCommand, ChangeMemberRoleResponse>
{
    private readonly AppDbContext _dbContext;

    public ChangeMemberRoleCommandHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ChangeMemberRoleResponse> Handle(
        ChangeMemberRoleCommand request,
        CancellationToken cancellationToken)
    {
        var actorMembership = await _dbContext.WorkspaceMembers
            .Include(member => member.Workspace)
            .FirstOrDefaultAsync(
                member =>
                    member.WorkspaceId == request.WorkspaceId &&
                    member.UserId == request.ActorUserId &&
                    member.Status == MembershipStatuses.Active,
                cancellationToken);

        if (actorMembership is null)
        {
            throw new NotFoundException("Workspace not found.");
        }

        if (actorMembership.Workspace.Status == ResourceStatuses.Archived)
        {
            throw new ConflictException("Archived workspace cannot be modified.");
        }

        if (actorMembership.Role is not WorkspaceMemberRoles.Owner and not WorkspaceMemberRoles.Admin)
        {
            throw new ForbiddenException("You do not have permission to change member roles.");
        }

        var targetMember = await _dbContext.WorkspaceMembers
            .FirstOrDefaultAsync(
                member =>
                    member.Id == request.MemberId &&
                    member.WorkspaceId == request.WorkspaceId &&
                    member.Status == MembershipStatuses.Active,
                cancellationToken);

        if (targetMember is null)
        {
            throw new NotFoundException("Workspace member not found.");
        }

        var newRole = request.Role.Trim().ToLowerInvariant();

        if (targetMember.Role == WorkspaceMemberRoles.Owner && newRole != WorkspaceMemberRoles.Owner)
        {
            var ownerCount = await _dbContext.WorkspaceMembers
                .CountAsync(
                    member =>
                        member.WorkspaceId == request.WorkspaceId &&
                        member.Role == WorkspaceMemberRoles.Owner &&
                        member.Status == MembershipStatuses.Active,
                    cancellationToken);

            if (ownerCount <= 1)
            {
                throw new ConflictException("Workspace must have at least one owner.");
            }
        }

        if (actorMembership.Role == WorkspaceMemberRoles.Admin && targetMember.Role == WorkspaceMemberRoles.Owner)
        {
            throw new ForbiddenException("Admin cannot change owner role.");
        }

        if (targetMember.Role == newRole)
        {
            throw new ConflictException("Member already has this role.");
        }

        targetMember.ChangeRole(newRole);

        if (newRole == WorkspaceMemberRoles.Admin)
        {
            var activeProjects = await _dbContext.Projects
                .Where(project =>
                    project.WorkspaceId == request.WorkspaceId &&
                    project.Status == ResourceStatuses.Active)
                .ToListAsync(cancellationToken);

            var activeProjectIds = activeProjects
                .Select(project => project.Id)
                .ToList();

            var existingProjectMembers = await _dbContext.ProjectMembers
                .Where(member =>
                    member.UserId == targetMember.UserId &&
                    activeProjectIds.Contains(member.ProjectId))
                .ToListAsync(cancellationToken);

            foreach (var existingProjectMember in existingProjectMembers)
            {
                existingProjectMember.ChangeRole(ProjectMemberRoles.Manager);
                existingProjectMember.Activate();
            }

            var existingProjectIds = existingProjectMembers
                .Select(member => member.ProjectId)
                .ToHashSet();

            var missingProjectMembers = activeProjects
                .Where(project => !existingProjectIds.Contains(project.Id))
                .Select(project => new ProjectMember(
                    project.Id,
                    targetMember.UserId,
                    ProjectMemberRoles.Manager))
                .ToList();

            _dbContext.ProjectMembers.AddRange(missingProjectMembers);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ChangeMemberRoleResponse(
            targetMember.Id,
            targetMember.UserId,
            targetMember.WorkspaceId,
            targetMember.Role,
            targetMember.Status,
            targetMember.UpdatedAt);
    }
}