using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.AccessControl;
using TraceFlow.Api.Application.Common.Exceptions;
using TraceFlow.Api.Application.Common.Logs;
using TraceFlow.Api.Domain.Constants;
using TraceFlow.Api.Infrastructure.Persistence;

namespace TraceFlow.Api.Application.Logs.Queries.SearchLogs;

public sealed class SearchLogsQueryHandler
    : IRequestHandler<SearchLogsQuery, SearchLogsResponse>
{
    private readonly AppDbContext _dbContext;
    private readonly ProjectAccessService _projectAccess;
    private readonly ILogSearchReader _logSearchReader;

    public SearchLogsQueryHandler(
        AppDbContext dbContext,
        ProjectAccessService projectAccess,
        ILogSearchReader logSearchReader)
    {
        _dbContext = dbContext;
        _projectAccess = projectAccess;
        _logSearchReader = logSearchReader;
    }

    public async Task<SearchLogsResponse> Handle(
        SearchLogsQuery request,
        CancellationToken cancellationToken)
    {
        var access = await _projectAccess.GetProjectAccessAsync(
            request.WorkspaceId,
            request.ProjectId,
            request.UserId,
            "Project not found.",
            cancellationToken,
            "Archived workspace cannot be accessed.");

        _projectAccess.EnsureProjectMember(
            access,
            "You do not have permission to search logs.");

        if (request.ApplicationId is not null)
        {
            var applicationExists = await _dbContext.TraceApplications
                .AsNoTracking()
                .AnyAsync(
                    application =>
                        application.Id == request.ApplicationId.Value &&
                        application.ProjectId == request.ProjectId &&
                        application.Status == ResourceStatuses.Active,
                    cancellationToken);

            if (!applicationExists)
            {
                throw new NotFoundException("Trace application not found.");
            }
        }

        var normalizedQuery = NormalizeTimeRange(request);

        return await _logSearchReader.SearchAsync(
            request,
            cancellationToken);
    }
    private static SearchLogsQuery NormalizeTimeRange(SearchLogsQuery query)
    {
        var to = query.To?.ToUniversalTime() ?? DateTime.UtcNow;
        var from = query.From?.ToUniversalTime() ?? to.AddHours(-24);

        return query with
        {
            From = from,
            To = to
        };
    }
}