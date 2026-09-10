using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.AccessControl;
using TraceFlow.Api.Application.Common.Exceptions;
using TraceFlow.Api.Domain.Constants;
using TraceFlow.Api.Domain.Entities;
using TraceFlow.Api.Infrastructure.Persistence;

namespace TraceFlow.Api.Application.TraceApplications.Commands.CreateTraceApplication;

public class CreateTraceApplicationCommandHandler
    : IRequestHandler<CreateTraceApplicationCommand, CreateTraceApplicationResponse>
{
    private readonly AppDbContext _dbContext;
    private readonly ProjectAccessService _projectAccess;

    public CreateTraceApplicationCommandHandler(
        AppDbContext dbContext,
        ProjectAccessService projectAccess)
    {
        _dbContext = dbContext;
        _projectAccess = projectAccess;
    }

    public async Task<CreateTraceApplicationResponse> Handle(
        CreateTraceApplicationCommand request,
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
            "You do not have permission to create trace applications.");

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
