namespace TraceFlow.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Authorize]
[Route("api/v{version:apiVersion}/workspaces")]
public class WorkspaceInvitationsController(ISender sender) : ControllerBase
{
    private readonly ISender _sender = sender;


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

    [HttpGet("invitations")]
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
    [HttpPost("invitations/{invitationId}/accept")]
    public async Task<IActionResult> AcceptWorkspaceInvitation(
        Ulid invitationId,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _sender.Send(
            new AcceptInvitationCommand(
                invitationId,
                userId.Value),
            cancellationToken);

        return Ok(result);
    }
    [HttpPost("invitations/{invitationId}/decline")]
    public async Task<IActionResult> DeclineInvitation(
        Ulid invitationId,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _sender.Send(
            new DeclineInvitationCommand(
                invitationId,
                userId.Value),
            cancellationToken);

        return Ok(result);
    }
    [HttpDelete("invitations/{invitationId}")]
    public async Task<IActionResult> CancelWorkspaceInvitation(
        Ulid workspaceId,
        Ulid invitationId,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _sender.Send(
            new CancelInvitationCommand(
                invitationId,
                userId.Value),
            cancellationToken);

        return Ok(result);
    }
}
