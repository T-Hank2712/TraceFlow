using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TraceFlow.Api.Application.Projects.Commands.CreateProject;
using TraceFlow.Api.Domain.Common.Extensions;
using TraceFlow.Api.Domain.Dtos.Projects;

namespace TraceFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/workspaces/{workspaceId}/projects")]
public class ProjectsController : ControllerBase
{
    private readonly ISender _sender;

    public ProjectsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> CreateProject(
        Ulid workspaceId,
        CreateProjectRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _sender.Send(
            new CreateProjectCommand(
                workspaceId,
                userId.Value,
                request.Name,
                request.Slug,
                request.Description),
            cancellationToken);

        return StatusCode(StatusCodes.Status201Created, result);
    }
}