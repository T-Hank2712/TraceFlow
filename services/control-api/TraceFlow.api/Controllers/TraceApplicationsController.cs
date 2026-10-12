namespace TraceFlow.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Authorize]
[Route("api/v{version:apiVersion}/workspaces/{workspaceId}/projects/{projectId}/applications")]
public class TraceApplicationsController(ISender sender) : ControllerBase
{
    private readonly ISender _sender = sender;


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
    [HttpGet]
    public async Task<IActionResult> ListTraceApplications(
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
            new ListTraceApplicationsQuery(
                workspaceId,
                projectId,
                userId.Value),
            cancellationToken);

        return Ok(result);
    }
    [HttpGet("{applicationId}")]
    public async Task<IActionResult> GetApplicationDetail(
        Ulid workspaceId,
        Ulid projectId,
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
                workspaceId,
                projectId,
                applicationId,
                userId.Value),
            cancellationToken);

        return Ok(result);
    }
    [HttpPatch("{applicationId}")]
    public async Task<IActionResult> UpdateApplication(
        Ulid workspaceId,
        Ulid projectId,
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
                workspaceId,
                projectId,
                applicationId,
                userId.Value,
                request.Name,
                request.Slug,
                request.Description),
            cancellationToken);

        return Ok(result);
    }
    [HttpDelete("{applicationId}")]
    public async Task<IActionResult> DeleteApplication(
        Ulid workspaceId,
        Ulid projectId,
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
                workspaceId,
                projectId,
                applicationId,
                userId.Value),
            cancellationToken);

        return Ok(result);
    }
}
