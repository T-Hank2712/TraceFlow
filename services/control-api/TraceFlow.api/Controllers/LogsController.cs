namespace TraceFlow.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Authorize]
[Route("api/v{version:apiVersion}/workspaces/{workspaceId}/projects/{projectId}/logs")]
public sealed class LogsController(ISender sender) : ControllerBase
{
    private readonly ISender _sender = sender;

    [EnableRateLimiting(RateLimitPolicies.Search)]
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
