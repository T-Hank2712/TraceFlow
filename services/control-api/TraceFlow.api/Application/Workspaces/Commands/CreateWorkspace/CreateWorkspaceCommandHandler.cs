using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Domain.Entities;
using TraceFlow.Api.Infrastructure.Persistence;
using TraceFlow.Api.Application.Common.Exceptions;
using TraceFlow.Api.Domain.Constants;

namespace TraceFlow.Api.Application.Workspaces.Commands.CreateWorkspace;

public class CreateWorkspaceCommandHandler
    : IRequestHandler<CreateWorkspaceCommand, CreateWorkspaceResponse>
{
    private readonly AppDbContext _dbContext;

    public CreateWorkspaceCommandHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CreateWorkspaceResponse> Handle(
        CreateWorkspaceCommand request,
        CancellationToken cancellationToken)
    {
        var normalizedSlug = request.Slug.Trim().ToLower();

        var slugExists = await _dbContext.Workspaces
            .AnyAsync(
                workspace =>
                    workspace.OwnerUserId == request.UserId &&
                    workspace.Slug == normalizedSlug &&
                    workspace.Status == WorkspaceStatuses.Active,
                cancellationToken);

        if (slugExists)
        {
            throw new ConflictException(
                "Workspace slug is already taken.");
        }

        var workspace = new Workspace(
            request.UserId,
            request.Name,
            request.Slug,
            request.Description);

        _dbContext.Workspaces.Add(workspace);

        var ownerMember = new WorkspaceMember(
            workspace.Id,
            request.UserId,
            WorkspaceMemberRoles.Owner);

        _dbContext.WorkspaceMembers.Add(ownerMember);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateWorkspaceResponse(
            workspace.Id,
            workspace.Name,
            workspace.Slug,
            workspace.Description,
            workspace.Status,
            ownerMember.Role);
    }
}