using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.Exceptions;
using TraceFlow.Api.Infrastructure.Persistence;
using TraceFlow.Api.Domain.Constants;

namespace TraceFlow.Api.Application.Workspaces.Commands.ArchiveWorkspace;

public class ArchiveWorkspaceCommandHandler
    : IRequestHandler<ArchiveWorkspaceCommand, ArchiveWorkspaceResponse>
{
    private readonly AppDbContext _dbContext;

    public ArchiveWorkspaceCommandHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ArchiveWorkspaceResponse> Handle(
        ArchiveWorkspaceCommand request,
        CancellationToken cancellationToken)
    {
        var membership = await _dbContext.WorkspaceMembers
            .Include(member => member.Workspace)
            .FirstOrDefaultAsync(
                member =>
                    member.WorkspaceId == request.WorkspaceId &&
                    member.UserId == request.UserId,
                cancellationToken);

        if (membership is null)
        {
            throw new NotFoundException("Workspace not found.");
        }

        if (membership.Role != WorkspaceMemberRoles.Owner)
        {
            throw new ForbiddenException(
                "Only workspace owner can archive this workspace.");
        }

        if (membership.Workspace.Status == WorkspaceStatuses.Archived)
        {
            throw new ConflictException("Archived workspace cannot be updated.");
        }

        membership.Workspace.Archive();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ArchiveWorkspaceResponse(
            membership.Workspace.Id,
            membership.Workspace.Name,
            membership.Workspace.Slug,
            membership.Workspace.Status,
            membership.Workspace.UpdatedAt);
    }
}