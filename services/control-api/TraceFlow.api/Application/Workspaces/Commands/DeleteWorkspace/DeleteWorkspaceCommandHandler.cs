using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.AccessControl;
using TraceFlow.Api.Domain.Constants;
using TraceFlow.Api.Infrastructure.Persistence;

namespace TraceFlow.Api.Application.Workspaces.Commands.DeleteWorkspace;

public class DeleteWorkspaceCommandHandler
    : IRequestHandler<DeleteWorkspaceCommand, DeleteWorkspaceResponse>
{
    private readonly AppDbContext _dbContext;
    private readonly WorkspaceAccessService _workspaceAccess;

    public DeleteWorkspaceCommandHandler(
        AppDbContext dbContext,
        WorkspaceAccessService workspaceAccess)
    {
        _dbContext = dbContext;
        _workspaceAccess = workspaceAccess;
    }

    public async Task<DeleteWorkspaceResponse> Handle(
        DeleteWorkspaceCommand request,
        CancellationToken cancellationToken)
    {
        var membership = await _workspaceAccess.GetActiveMembershipAsync(
            request.WorkspaceId,
            request.UserId,
            "Workspace not found.",
            cancellationToken);

        _workspaceAccess.EnsureWorkspaceOwner(
            membership,
            "Only workspace owner can delete this workspace.");

        var workspace = membership.Workspace;

        _workspaceAccess.EnsureWorkspaceIsActive(
            workspace,
            "Workspace is already archived.");

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
