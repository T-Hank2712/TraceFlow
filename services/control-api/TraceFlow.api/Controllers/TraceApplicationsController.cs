using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TraceFlow.Api.Application.TraceApplications.Commands.CreateTraceApplication;
using TraceFlow.Api.Domain.Common.Extensions;
using TraceFlow.Api.Domain.Dtos.TraceApplications;

namespace TraceFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/workspaces/{workspaceId}/projects/{projectId}/applications")]
public class TraceApplicationsController : ControllerBase
{
    private readonly ISender _sender;

    public TraceApplicationsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> CreateTraceApplication(
        Ulid workspaceId,
        Ulid projectId,
        CreateTraceApplicationRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _sender.Send(
            new CreateTraceApplicationCommand(
                workspaceId,
                projectId,
                userId.Value,
                request.Name,
                request.Slug,
                request.Description),
            cancellationToken);

        return StatusCode(StatusCodes.Status201Created, result);
    }
}