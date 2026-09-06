using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.Exceptions;
using TraceFlow.Api.Infrastructure.Persistence;

namespace TraceFlow.Api.Application.Workspaces.Queries.GetWorkspaceById;

public class GetWorkspaceByIdQueryHandler
    : IRequestHandler<GetWorkspaceByIdQuery, WorkspaceDetailResponse>
{
    private readonly AppDbContext _dbContext;

    public GetWorkspaceByIdQueryHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<WorkspaceDetailResponse> Handle(
        GetWorkspaceByIdQuery request,
        CancellationToken cancellationToken)
    {
        var membership = await _dbContext.WorkspaceMembers
            .AsNoTracking()
            .Where(member =>
                member.WorkspaceId == request.WorkspaceId &&
                member.UserId == request.UserId)
            .Select(member => new
            {
                member.Role,
                Workspace = member.Workspace
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (membership is null)
        {
            throw new NotFoundException("Workspace not found.");
        }

        return new WorkspaceDetailResponse(
            membership.Workspace.Id,
            membership.Workspace.Name,
            membership.Workspace.Slug,
            membership.Workspace.Description,
            membership.Workspace.Status,
            membership.Role,
            membership.Workspace.CreatedAt,
            membership.Workspace.UpdatedAt
        );
    }
}