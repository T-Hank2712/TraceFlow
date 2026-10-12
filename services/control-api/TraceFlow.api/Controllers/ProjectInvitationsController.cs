namespace TraceFlow.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Authorize]
public class ProjectInvitationsController(ISender sender) : ControllerBase
{
    private readonly ISender _sender = sender;


    [HttpPost("api/v{version:apiVersion}/workspaces/{workspaceId}/projects/{projectId}/invitations")]
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
    [HttpGet("api/v{version:apiVersion}/project-invitations")]
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
    [HttpGet("api/v{version:apiVersion}/workspaces/{workspaceId}/projects/{projectId}/invitations")]
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
    [HttpPost("api/v{version:apiVersion}/project-invitations/{invitationId}/accept")]
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
    [HttpPost("api/v{version:apiVersion}/project-invitations/{invitationId}/decline")]
    public async Task<IActionResult> DeclineProjectInvitation(
        Ulid invitationId,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _sender.Send(
            new DeclineProjectInvitationCommand(
                invitationId,
                userId.Value),
            cancellationToken);

        return Ok(result);
    }
    [HttpDelete("api/v{version:apiVersion}/workspaces/{workspaceId}/projects/{projectId}/invitations/{invitationId}")]
    public async Task<IActionResult> CancelProjectInvitation(
        Ulid workspaceId,
        Ulid projectId,
        Ulid invitationId,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _sender.Send(
            new CancelProjectInvitationCommand(
                workspaceId,
                projectId,
                invitationId,
                userId.Value),
            cancellationToken);

        return Ok(result);
    }
}
