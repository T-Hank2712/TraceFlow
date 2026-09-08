using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.Exceptions;
using TraceFlow.Api.Infrastructure.Persistence;
using TraceFlow.Api.Domain.Constants;

namespace TraceFlow.Api.Application.Workspaces.Commands.UpdateWorkspace;

public class UpdateWorkspaceCommandHandler
    : IRequestHandler<UpdateWorkspaceCommand, UpdateWorkspaceResponse>
{
    private readonly AppDbContext _dbContext;

    public UpdateWorkspaceCommandHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UpdateWorkspaceResponse> Handle(
        UpdateWorkspaceCommand request,
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

        if (membership.Role is not WorkspaceMemberRoles.Owner and not WorkspaceMemberRoles.Admin)
        {
            throw new ForbiddenException(
                "You do not have permission to update this workspace.");
        }

        if (membership.Workspace.Status == ResourceStatuses.Archived)
        {
            throw new ConflictException("Archived workspace cannot be updated.");
        }

        if (!string.IsNullOrWhiteSpace(request.Slug))
        {
            var normalizedSlug = request.Slug.Trim().ToLowerInvariant();

            var slugExists = await _dbContext.Workspaces
                .AnyAsync(
                    workspace =>
                        workspace.OwnerUserId == membership.Workspace.OwnerUserId &&
                        workspace.Slug == normalizedSlug &&
                        workspace.Status == ResourceStatuses.Active &&
                        workspace.Id != request.WorkspaceId,
                    cancellationToken);

            if (slugExists)
            {
                throw new ConflictException("Workspace slug is already taken.");
            }
        }

        membership.Workspace.UpdateWorkspace(
            request.Name,
            request.Slug,
            request.Description);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateWorkspaceResponse(
            membership.Workspace.Id,
            membership.Workspace.Name,
            membership.Workspace.Slug,
            membership.Workspace.Description,
            membership.Workspace.Status,
            membership.Workspace.CreatedAt,
            membership.Workspace.UpdatedAt);
    }
}