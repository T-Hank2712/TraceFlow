using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.Exceptions;
using TraceFlow.Api.Domain.Constants;
using TraceFlow.Api.Infrastructure.Persistence;

namespace TraceFlow.Api.Application.Projects.Commands.RemoveProjectMember;

public class RemoveProjectMemberCommandHandler
    : IRequestHandler<RemoveProjectMemberCommand, RemoveProjectMemberResponse>
{
    private readonly AppDbContext _dbContext;

    public RemoveProjectMemberCommandHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RemoveProjectMemberResponse> Handle(
        RemoveProjectMemberCommand request,
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
            throw new ForbiddenException("You do not have permission to remove project members.");
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

        _dbContext.ProjectMembers.Remove(targetMember);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return response;
    }
}