using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TraceFlow.Api.Application.Logs.Queries.SearchLogs;
using TraceFlow.Api.Domain.Common.Extensions;
using TraceFlow.Api.Domain.Dtos.Logs;

namespace TraceFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/workspaces/{workspaceId}/projects/{projectId}/logs")]
public sealed class LogsController : ControllerBase
{
    private readonly ISender _sender;

    public LogsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> SearchLogs(
        Ulid workspaceId,
        Ulid projectId,
        [FromQuery] SearchLogsRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _sender.Send(
            new SearchLogsQuery(
                workspaceId,
                projectId,
                userId.Value,
                request.ApplicationId,
                request.Environment,
                request.Level,
                request.Service,
                request.TraceId,
                request.CorrelationId,
                request.From,
                request.To,
                request.Page,
                request.PageSize),
            cancellationToken);

        return Ok(result);
    }
}