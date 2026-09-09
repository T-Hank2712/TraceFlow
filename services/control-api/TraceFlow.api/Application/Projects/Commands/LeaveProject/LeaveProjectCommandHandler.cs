using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.Exceptions;
using TraceFlow.Api.Domain.Constants;
using TraceFlow.Api.Infrastructure.Persistence;

namespace TraceFlow.Api.Application.Projects.Commands.LeaveProject;

public class LeaveProjectCommandHandler
    : IRequestHandler<LeaveProjectCommand, LeaveProjectResponse>
{
    private readonly AppDbContext _dbContext;

    public LeaveProjectCommandHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<LeaveProjectResponse> Handle(
        LeaveProjectCommand request,
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
            throw new ConflictException("Archived workspace cannot be accessed.");
        }

        var project = await _dbContext.Projects
            .AsNoTracking()
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

        var projectMember = await _dbContext.ProjectMembers
            .FirstOrDefaultAsync(
                member =>
                    member.ProjectId == request.ProjectId &&
                    member.UserId == request.UserId &&
                    member.Status == MembershipStatuses.Active,
                cancellationToken);

        if (projectMember is null)
        {
            throw new NotFoundException("Project not found.");
        }

        if (projectMember.Role == ProjectMemberRoles.Manager)
        {
            var managerCount = await _dbContext.ProjectMembers
                .CountAsync(
                    member =>
                        member.ProjectId == request.ProjectId &&
                        member.Role == ProjectMemberRoles.Manager &&
                        member.Status == MembershipStatuses.Active,
                    cancellationToken);

            if (managerCount <= 1)
            {
                throw new ConflictException("Project must have at least one manager.");
            }
        }

        _dbContext.ProjectMembers.Remove(projectMember);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new LeaveProjectResponse(
            request.WorkspaceId,
            request.ProjectId,
            request.UserId,
            "Left project successfully.");
    }
}