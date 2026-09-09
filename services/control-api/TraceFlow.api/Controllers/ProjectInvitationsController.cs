using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TraceFlow.Api.Application.Projects.Commands.InviteProjectMember;
using TraceFlow.Api.Domain.Common.Extensions;
using TraceFlow.Api.Domain.Dtos.Projects;

namespace TraceFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/workspaces/{workspaceId}/projects")]
public class ProjectInvitationsController : ControllerBase
{
    private readonly ISender _sender;

    public ProjectInvitationsController(ISender sender)
    {
        _sender = sender;
    }
    [HttpPost("{projectId}/invitations")]
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
}