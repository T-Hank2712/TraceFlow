using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.AccessControl;
using TraceFlow.Api.Application.Common.Exceptions;
using TraceFlow.Api.Domain.Constants;
using TraceFlow.Api.Infrastructure.Persistence;

namespace TraceFlow.Api.Application.TraceApplications.Commands.UpdateApplication;

public class UpdateApplicationCommandHandler
    : IRequestHandler<UpdateApplicationCommand, UpdateApplicationResponse>
{
    private readonly AppDbContext _dbContext;
    private readonly ProjectAccessService _projectAccess;

    public UpdateApplicationCommandHandler(
        AppDbContext dbContext,
        ProjectAccessService projectAccess)
    {
        _dbContext = dbContext;
        _projectAccess = projectAccess;
    }

    public async Task<UpdateApplicationResponse> Handle(
        UpdateApplicationCommand request,
        CancellationToken cancellationToken)
    {
        var access = await _projectAccess.GetProjectAccessAsync(
            request.WorkspaceId,
            request.ProjectId,
            request.UserId,
            "Project not found.",
            cancellationToken);

        _projectAccess.EnsureProjectDeveloperOrManager(
            access,
            "You do not have permission to update this application.");

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
