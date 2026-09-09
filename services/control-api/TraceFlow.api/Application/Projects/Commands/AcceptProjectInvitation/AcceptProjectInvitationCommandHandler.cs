using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.Exceptions;
using TraceFlow.Api.Domain.Constants;
using TraceFlow.Api.Domain.Entities;
using TraceFlow.Api.Infrastructure.Persistence;

namespace TraceFlow.Api.Application.Projects.Commands.AcceptProjectInvitation;

public class AcceptProjectInvitationCommandHandler
    : IRequestHandler<AcceptProjectInvitationCommand, AcceptProjectInvitationResponse>
{
    private readonly AppDbContext _dbContext;

    public AcceptProjectInvitationCommandHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AcceptProjectInvitationResponse> Handle(
        AcceptProjectInvitationCommand request,
        CancellationToken cancellationToken)
    {
        var invitation = await _dbContext.ProjectInvitations
            .Include(invitation => invitation.Workspace)
            .Include(invitation => invitation.Project)
            .FirstOrDefaultAsync(
                invitation =>
                    invitation.Id == request.InvitationId &&
                    invitation.InvitedUserId == request.UserId,
                cancellationToken);

        if (invitation is null)
        {
            throw new NotFoundException("Invitation not found.");
        }

        if (invitation.Workspace.Status == ResourceStatuses.Archived)
        {
            throw new ConflictException("Archived workspace cannot be joined.");
        }

        var workspaceMember = await _dbContext.WorkspaceMembers
            .FirstOrDefaultAsync(
                member =>
                    member.WorkspaceId == invitation.WorkspaceId &&
                    member.UserId == request.UserId,
                cancellationToken);

        if (workspaceMember is null)
        {
            workspaceMember = new WorkspaceMember(
                invitation.WorkspaceId,
                request.UserId,
                WorkspaceMemberRoles.Member);

            _dbContext.WorkspaceMembers.Add(workspaceMember);
        }
        else if (workspaceMember.Status != MembershipStatuses.Active)
        {
            workspaceMember.Activate();
            workspaceMember.ChangeRole(WorkspaceMemberRoles.Member);
        }

        var existingProjectMember = await _dbContext.ProjectMembers
            .FirstOrDefaultAsync(
                member =>
                    member.ProjectId == invitation.ProjectId &&
                    member.UserId == request.UserId,
                cancellationToken);

        if (existingProjectMember is not null &&
            existingProjectMember.Status == MembershipStatuses.Active)
        {
            throw new ConflictException("User is already a project member.");
        }

        ProjectMember projectMember;

        if (existingProjectMember is null)
        {
            projectMember = new ProjectMember(
                invitation.ProjectId,
                request.UserId,
                invitation.Role);

            _dbContext.ProjectMembers.Add(projectMember);
        }
        else
        {
            existingProjectMember.Activate();
            existingProjectMember.ChangeRole(invitation.Role);
            projectMember = existingProjectMember;
        }

        invitation.Accept();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AcceptProjectInvitationResponse(
            invitation.Id,
            invitation.WorkspaceId,
            invitation.ProjectId,
            workspaceMember.Id,
            projectMember.Id,
            projectMember.Role,
            invitation.Status);
    }
}