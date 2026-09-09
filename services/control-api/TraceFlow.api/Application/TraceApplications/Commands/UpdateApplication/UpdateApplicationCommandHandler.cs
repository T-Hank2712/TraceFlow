using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.Exceptions;
using TraceFlow.Api.Domain.Constants;
using TraceFlow.Api.Infrastructure.Persistence;

namespace TraceFlow.Api.Application.TraceApplications.Commands.UpdateApplication;

public class UpdateApplicationCommandHandler
    : IRequestHandler<UpdateApplicationCommand, UpdateApplicationResponse>
{
    private readonly AppDbContext _dbContext;

    public UpdateApplicationCommandHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UpdateApplicationResponse> Handle(
        UpdateApplicationCommand request,
        CancellationToken cancellationToken)
    {
        var workspaceMember = await _dbContext.WorkspaceMembers
            .FirstOrDefaultAsync(x =>
                x.WorkspaceId == request.WorkspaceId &&
                x.UserId == request.UserId &&
                x.Status == MembershipStatuses.Active,
                cancellationToken);

        if (workspaceMember is null)
        {
            throw new NotFoundException("Workspace not found.");
        }

        var workspace = await _dbContext.Workspaces
            .FirstOrDefaultAsync(x =>
                x.Id == request.WorkspaceId &&
                x.Status == ResourceStatuses.Active,
                cancellationToken);

        if (workspace is null)
        {
            throw new NotFoundException("Workspace not found.");
        }

        var project = await _dbContext.Projects
            .FirstOrDefaultAsync(x =>
                x.Id == request.ProjectId &&
                x.WorkspaceId == request.WorkspaceId &&
                x.Status == ResourceStatuses.Active,
                cancellationToken);

        if (project is null)
        {
            throw new NotFoundException("Project not found.");
        }

        var isWorkspaceOwnerOrAdmin =
            workspaceMember.Role == WorkspaceMemberRoles.Owner ||
            workspaceMember.Role == WorkspaceMemberRoles.Admin;

        var projectMember = await _dbContext.ProjectMembers
            .FirstOrDefaultAsync(x =>
                x.ProjectId == request.ProjectId &&
                x.UserId == request.UserId &&
                x.Status == MembershipStatuses.Active,
                cancellationToken);

        var canUpdateProjectApplication =
            isWorkspaceOwnerOrAdmin ||
            projectMember?.Role == ProjectMemberRoles.Manager ||
            projectMember?.Role == ProjectMemberRoles.Developer;

        if (!canUpdateProjectApplication)
        {
            throw new ForbiddenException("You do not have permission to update this application.");
        }

        var application = await _dbContext.TraceApplications
            .FirstOrDefaultAsync(x =>
                x.Id == request.ApplicationId &&
                x.ProjectId == request.ProjectId &&
                x.Status == ResourceStatuses.Active,
                cancellationToken);

        if (application is null)
        {
            throw new NotFoundException("Application not found.");
        }

        if (!string.IsNullOrWhiteSpace(request.Slug))
        {
            var normalizedSlug = request.Slug.Trim().ToLowerInvariant();

            var slugAlreadyTaken = await _dbContext.TraceApplications
                .AnyAsync(x =>
                    x.Id != request.ApplicationId &&
                    x.ProjectId == request.ProjectId &&
                    x.Slug == normalizedSlug &&
                    x.Status == ResourceStatuses.Active,
                    cancellationToken);

            if (slugAlreadyTaken)
            {
                throw new ConflictException("Application slug is already taken.");
            }
        }

        application.Update(
            request.Name,
            request.Slug,
            request.Description);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateApplicationResponse(
            application.Id,
            application.ProjectId,
            application.Name,
            application.Slug,
            application.Description,
            application.Status,
            application.UpdatedAt);
    }
}