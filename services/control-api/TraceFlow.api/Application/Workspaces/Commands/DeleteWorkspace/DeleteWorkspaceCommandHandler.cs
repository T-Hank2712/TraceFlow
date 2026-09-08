using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.Exceptions;
using TraceFlow.Api.Domain.Constants;
using TraceFlow.Api.Infrastructure.Persistence;

namespace TraceFlow.Api.Application.Workspaces.Commands.DeleteWorkspace;

public class DeleteWorkspaceCommandHandler
    : IRequestHandler<DeleteWorkspaceCommand, DeleteWorkspaceResponse>
{
    private readonly AppDbContext _dbContext;

    public DeleteWorkspaceCommandHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DeleteWorkspaceResponse> Handle(
        DeleteWorkspaceCommand request,
        CancellationToken cancellationToken)
    {
        var membership = await _dbContext.WorkspaceMembers
            .Include(member => member.Workspace)
            .FirstOrDefaultAsync(
                member =>
                    member.WorkspaceId == request.WorkspaceId &&
                    member.UserId == request.UserId &&
                    member.Status == WorkspaceMemberStatuses.Active,
                cancellationToken);

        if (membership is null)
        {
            throw new NotFoundException("Workspace not found.");
        }

        if (membership.Role != WorkspaceMemberRoles.Owner)
        {
            throw new ForbiddenException("Only workspace owner can delete this workspace.");
        }

        var workspace = membership.Workspace;

        if (workspace.Status == ResourceStatuses.Archived)
        {
            throw new ConflictException("Workspace is already archived.");
        }

        var memberCount = await _dbContext.WorkspaceMembers
            .CountAsync(
                member => member.WorkspaceId == request.WorkspaceId,
                cancellationToken);

        var hasInvitations = await _dbContext.WorkspaceInvitations
            .AnyAsync(
                invitation => invitation.WorkspaceId == request.WorkspaceId,
                cancellationToken);

        var canHardDelete =
            memberCount == 1 &&
            !hasInvitations;

        if (canHardDelete)
        {
            _dbContext.Workspaces.Remove(workspace);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new DeleteWorkspaceResponse(
                workspace.Id,
                DeleteMode.Hard,
                "Workspace permanently deleted.");
        }

        workspace.Archive();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new DeleteWorkspaceResponse(
            workspace.Id,
            DeleteMode.Soft,
            "Workspace archived.");
    }
}