using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Infrastructure.Persistence;

namespace TraceFlow.Api.Application.Workspaces.Queries.GetWorkspaces;

public class GetMyWorkspacesQueryHandler
    : IRequestHandler<GetWorkspacesQuery, IReadOnlyList<WorkspaceSummaryResponse>>
{
    private readonly AppDbContext _dbContext;

    public GetMyWorkspacesQueryHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<WorkspaceSummaryResponse>> Handle(
        GetWorkspacesQuery request,
        CancellationToken cancellationToken)
    {
        return await _dbContext.WorkspaceMembers
            .AsNoTracking()
            .Where(member => member.UserId == request.UserId)
            .Include(member => member.Workspace)
            .OrderByDescending(member => member.Workspace.CreatedAt)
            .Select(member => new WorkspaceSummaryResponse(
                member.Workspace.Id,
                member.Workspace.Name,
                member.Workspace.Slug,
                member.Workspace.Description,
                member.Role,
                member.Workspace.Status,
                member.Workspace.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}