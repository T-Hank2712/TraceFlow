using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.Exceptions;
using TraceFlow.Api.Infrastructure.Persistence;

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

        if (membership.Role is not "owner" and not "admin")
        {
            throw new UnauthorizedAccessException(
                "You do not have permission to update this workspace.");
        }

        if (!string.IsNullOrWhiteSpace(request.Slug))
        {
            var normalizedSlug = request.Slug.Trim().ToLowerInvariant();

            var slugExists = await _dbContext.Workspaces
                .AnyAsync(
                    workspace =>
                        workspace.Slug == normalizedSlug &&
                        workspace.Id != request.WorkspaceId,
                    cancellationToken);

            if (slugExists)
            {
                throw new InvalidOperationException("Workspace slug is already taken.");
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