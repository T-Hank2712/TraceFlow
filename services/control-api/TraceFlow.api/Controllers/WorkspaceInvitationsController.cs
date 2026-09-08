using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TraceFlow.Api.Domain.Common.Extensions;
using TraceFlow.Api.Application.Workspaces.Commands.InviteWorkspaceMember;
using TraceFlow.Api.Domain.Dtos.Workspaces;
using TraceFlow.Api.Application.Workspaces.Queries.InvitationSent;
using TraceFlow.Api.Application.Workspaces.Queries.InvitationInbox;

namespace TraceFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/workspace-invitations")]
public class WorkspaceInvitationsController : ControllerBase
{
    private readonly ISender _sender;

    public WorkspaceInvitationsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("{workspaceId}/invitations")]
    public async Task<IActionResult> InviteWorkspaceMember(
        Ulid workspaceId,
        InviteWorkspaceMemberRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _sender.Send(
            new InviteWorkspaceMemberCommand(
                workspaceId,
                userId.Value,
                request.Identifier,
                request.Role),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetMyWorkspaceInvitations(
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _sender.Send(
            new InvitationInboxQuery(userId.Value),
            cancellationToken);

        return Ok(result);
    }
    [HttpGet("{workspaceId}/invitations")]
    public async Task<IActionResult> GetWorkspaceSentInvitations(
        Ulid workspaceId,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _sender.Send(
            new InvitationSentQuery(
                workspaceId,
                userId.Value),
            cancellationToken);

        return Ok(result);
    }
}