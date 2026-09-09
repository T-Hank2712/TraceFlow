using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TraceFlow.Api.Application.Projects.Commands.InviteProjectMember;
using TraceFlow.Api.Domain.Common.Extensions;
using TraceFlow.Api.Domain.Dtos.Projects;
using TraceFlow.Api.Application.Projects.Queries.ProjectInvitationInbox;
using TraceFlow.Api.Application.Projects.Queries.ProjectInvitationSent;
using TraceFlow.Api.Application.Projects.Commands.AcceptProjectInvitation;

namespace TraceFlow.Api.Controllers;

[ApiController]
[Authorize]
public class ProjectInvitationsController : ControllerBase
{
    private readonly ISender _sender;

    public ProjectInvitationsController(ISender sender)
    {
        _sender = sender;
    }
    [HttpPost("api/workspaces/{workspaceId}/projects/{projectId}/invitations")]
    public async Task<IActionResult> InviteProjectMember(
        Ulid workspaceId,
        Ulid projectId,
        InviteProjectMemberRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _sender.Send(
            new InviteProjectMemberCommand(
                workspaceId,
                projectId,
                userId.Value,
                request.Identifier,
                request.Role),
            cancellationToken);

        return Ok(result);
    }
    [HttpGet("api/project-invitations")]
    public async Task<IActionResult> GetProjectInvitationInbox(
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _sender.Send(
            new ProjectInvitationInboxQuery(userId.Value),
            cancellationToken);

        return Ok(result);
    }
    [HttpGet("api/workspaces/{workspaceId}/projects/{projectId}/invitations")]
    public async Task<IActionResult> GetProjectInvitationSent(
        Ulid workspaceId,
        Ulid projectId,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _sender.Send(
            new ProjectInvitationSentQuery(
                workspaceId,
                projectId,
                userId.Value),
            cancellationToken);

        return Ok(result);
    }
    [HttpPost("api/project-invitations/{invitationId}/accept")]
    public async Task<IActionResult> AcceptProjectInvitation(
        Ulid invitationId,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _sender.Send(
            new AcceptProjectInvitationCommand(
                invitationId,
                userId.Value),
            cancellationToken);

        return Ok(result);
    }
}