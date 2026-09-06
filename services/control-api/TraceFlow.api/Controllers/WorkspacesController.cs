using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TraceFlow.Api.Application.Workspaces.Commands.CreateWorkspace;
using TraceFlow.Api.Domain.Dtos;
using TraceFlow.Api.Domain.Common.Extensions;

namespace TraceFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/workspaces")]
public class WorkspacesController : ControllerBase
{
    private readonly ISender _sender;

    public WorkspacesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> CreateWorkspace(
        CreateWorkspaceRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _sender.Send(
            new CreateWorkspaceCommand(
                userId.Value,
                request.Name,
                request.Slug,
                request.Description),
            cancellationToken);

        return CreatedAtAction(
            nameof(CreateWorkspace),
            new { id = result.Id },
            result);
    }
}