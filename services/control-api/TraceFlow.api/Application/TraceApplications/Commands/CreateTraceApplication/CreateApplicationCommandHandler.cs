using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.Exceptions;
using TraceFlow.Api.Domain.Constants;
using TraceFlow.Api.Domain.Entities;
using TraceFlow.Api.Infrastructure.Persistence;

namespace TraceFlow.Api.Application.TraceApplications.Commands.CreateTraceApplication;

public class CreateTraceApplicationCommandHandler
    : IRequestHandler<CreateTraceApplicationCommand, CreateTraceApplicationResponse>
{
    private readonly AppDbContext _dbContext;

    public CreateTraceApplicationCommandHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CreateTraceApplicationResponse> Handle(
        CreateTraceApplicationCommand request,
        CancellationToken cancellationToken)
    {
        var workspaceMembership = await _dbContext.WorkspaceMembers
            .Include(member => member.Workspace)
            .FirstOrDefaultAsync(
                member =>
                    member.WorkspaceId == request.WorkspaceId &&
                    member.UserId == request.UserId &&
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

        var isWorkspaceManager =
            workspaceMembership.Role is WorkspaceMemberRoles.Owner or WorkspaceMemberRoles.Admin;

        var projectMembership = await _dbContext.ProjectMembers
            .AsNoTracking()
            .FirstOrDefaultAsync(
                member =>
                    member.ProjectId == request.ProjectId &&
                    member.UserId == request.UserId &&
                    member.Status == MembershipStatuses.Active,
                cancellationToken);

        var canCreateApplication =
            isWorkspaceManager ||
            projectMembership?.Role is ProjectMemberRoles.Manager or ProjectMemberRoles.Developer;

        if (!canCreateApplication)
        {
            throw new ForbiddenException("You do not have permission to create trace applications.");
        }

        var normalizedSlug = request.Slug.Trim().ToLowerInvariant();

        var slugExists = await _dbContext.TraceApplications
            .AnyAsync(
                application =>
                    application.ProjectId == request.ProjectId &&
                    application.Slug == normalizedSlug &&
                    application.Status == ResourceStatuses.Active,
                cancellationToken);

        if (slugExists)
        {
            throw new ConflictException("Trace application slug is already taken.");
        }

        var traceApplication = new TraceApplication(
            request.ProjectId,
            request.UserId,
            request.Name,
            request.Slug,
            request.Description);

        _dbContext.TraceApplications.Add(traceApplication);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateTraceApplicationResponse(
            traceApplication.Id,
            traceApplication.ProjectId,
            traceApplication.CreatedByUserId,
            traceApplication.Name,
            traceApplication.Slug,
            traceApplication.Description,
            traceApplication.Status,
            traceApplication.CreatedAt);
    }
}