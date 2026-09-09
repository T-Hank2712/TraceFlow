using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.Exceptions;
using TraceFlow.Api.Domain.Constants;
using TraceFlow.Api.Infrastructure.Persistence;

namespace TraceFlow.Api.Application.Workspaces.Commands.LeaveWorkspace;

public class LeaveWorkspaceCommandHandler
    : IRequestHandler<LeaveWorkspaceCommand, LeaveWorkspaceResponse>
{
    private readonly AppDbContext _dbContext;

    public LeaveWorkspaceCommandHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<LeaveWorkspaceResponse> Handle(
        LeaveWorkspaceCommand request,
        CancellationToken cancellationToken)
    {
        var membership = await _dbContext.WorkspaceMembers
            .Include(member => member.Workspace)
            .FirstOrDefaultAsync(
                member =>
                    member.WorkspaceId == request.WorkspaceId &&
                    member.UserId == request.UserId &&
                    member.Status == MembershipStatuses.Active,
                cancellationToken);

        if (membership is null)
        {
            throw new NotFoundException("Workspace not found.");
        }

        if (membership.Workspace.Status == ResourceStatuses.Archived)
        {
            throw new ConflictException("Archived workspace cannot be left.");
        }

        if (membership.Role == WorkspaceMemberRoles.Owner)
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

        var projectMembers = await _dbContext.ProjectMembers
            .Where(projectMember =>
                projectMember.UserId == request.UserId &&
                projectMember.Project.WorkspaceId == request.WorkspaceId)
            .ToListAsync(cancellationToken);

        _dbContext.ProjectMembers.RemoveRange(projectMembers);

        _dbContext.WorkspaceMembers.Remove(membership);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new LeaveWorkspaceResponse(
            request.WorkspaceId,
            request.UserId,
            "Left workspace successfully.");
    }
}