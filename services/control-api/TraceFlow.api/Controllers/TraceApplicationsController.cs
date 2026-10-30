namespace TraceFlow.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Authorize]
[Route("api/v{version:apiVersion}/projects/{projectId}/applications")]
public class TraceApplicationsController(ISender sender) : ControllerBase
{
    private readonly ISender _sender = sender;
    [HttpPost]
    public async Task<IActionResult> CreateTraceApplication(
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
                projectId,
                userId.Value,
                request.Name,
                request.Slug,
                request.Description),
            cancellationToken);

        return StatusCode(StatusCodes.Status201Created, result);
    }
    [HttpGet]
    public async Task<IActionResult> ListTraceApplications(
        Ulid projectId,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _sender.Send(
            new ListTraceApplicationsQuery(
                projectId,
                userId.Value),
            cancellationToken);

        return Ok(result);
    }
    [HttpGet("~/api/v{version:apiVersion}/applications/{applicationId}")]
    public async Task<IActionResult> GetApplicationDetail(
        Ulid applicationId,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _sender.Send(
            new GetApplicationDetailQuery(
                applicationId,
                userId.Value),
            cancellationToken);

        return Ok(result);
    }
    [HttpPatch("~/api/v{version:apiVersion}/applications/{applicationId}")]
    public async Task<IActionResult> UpdateApplication(
        Ulid applicationId,
        UpdateApplicationRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _sender.Send(
            new UpdateApplicationCommand(
                applicationId,
                userId.Value,
                request.Name,
                request.Slug,
                request.Description),
            cancellationToken);

        return Ok(result);
    }
    [HttpDelete("~/api/v{version:apiVersion}/applications/{applicationId}")]
    public async Task<IActionResult> DeleteApplication(
        Ulid applicationId,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _sender.Send(
            new DeleteApplicationCommand(
                applicationId,
                userId.Value),
            cancellationToken);

        return Ok(result);
    }
}
