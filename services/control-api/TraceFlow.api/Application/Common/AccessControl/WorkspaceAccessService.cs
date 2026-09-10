using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.Exceptions;
using TraceFlow.Api.Domain.Constants;
using TraceFlow.Api.Domain.Entities;
using TraceFlow.Api.Infrastructure.Persistence;

namespace TraceFlow.Api.Application.Common.AccessControl;

public class WorkspaceAccessService
{
    private readonly AppDbContext _dbContext;

    public WorkspaceAccessService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<WorkspaceMember> GetActiveMembershipAsync(
        Ulid workspaceId,
        Ulid userId,
        string notFoundMessage,
        CancellationToken cancellationToken)
    {
        var membership = await _dbContext.WorkspaceMembers
            .Include(member => member.Workspace)
            .FirstOrDefaultAsync(
                member =>
                    member.WorkspaceId == workspaceId &&
                    member.UserId == userId &&
                    member.Status == MembershipStatuses.Active,
                cancellationToken);

        if (membership is null)
        {
            throw new NotFoundException(notFoundMessage);
        }

        return membership;
    }

    public void EnsureWorkspaceIsActive(
        Workspace workspace,
        string conflictMessage)
    {
        if (workspace.Status == ResourceStatuses.Archived)
        {
            throw new ConflictException(conflictMessage);
        }
    }

    public void EnsureWorkspaceManager(
        WorkspaceMember membership,
        string forbiddenMessage)
    {
        if (membership.Role is not WorkspaceMemberRoles.Owner and not WorkspaceMemberRoles.Admin)
        {
            throw new ForbiddenException(forbiddenMessage);
        }
    }

    public void EnsureWorkspaceOwner(
        WorkspaceMember membership,
        string forbiddenMessage)
    {
        if (membership.Role != WorkspaceMemberRoles.Owner)
        {
            throw new ForbiddenException(forbiddenMessage);
        }
    }

    public static bool IsWorkspaceManager(WorkspaceMember membership)
    {
        return membership.Role is WorkspaceMemberRoles.Owner or WorkspaceMemberRoles.Admin;
    }
}