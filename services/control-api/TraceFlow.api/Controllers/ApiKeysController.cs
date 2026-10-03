namespace TraceFlow.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Authorize]
[Route("api/v{version:apiVersion}/workspaces/{workspaceId}/projects/{projectId}/applications/{applicationId}/api-keys")]
public class ApiKeysController(ISender sender) : ControllerBase
{
    private readonly ISender _sender = sender;


    [HttpPost]
    public async Task<IActionResult> CreateApiKey(
        Ulid workspaceId,
        Ulid projectId,
        Ulid applicationId,
        CreateApiKeyRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _sender.Send(
            new CreateApiKeyCommand(
                workspaceId,
                projectId,
                applicationId,
                userId.Value,
                request.Name,
                request.Environment,
                request.ExpirationPolicy),
            cancellationToken);

        return StatusCode(StatusCodes.Status201Created, result);
    }
    [HttpGet]
    public async Task<IActionResult> ListApiKeys(
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
            new ListApiKeysQuery(
                workspaceId,
                projectId,
                applicationId,
                userId.Value),
            cancellationToken);

        return Ok(result);
    }
    [HttpDelete("{apiKeyId}")]
    public async Task<IActionResult> RevokeApiKey(
        Ulid workspaceId,
        Ulid projectId,
        Ulid applicationId,
        Ulid apiKeyId,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _sender.Send(
            new RevokeApiKeyCommand(
                workspaceId,
                projectId,
                applicationId,
                apiKeyId,
                userId.Value),
            cancellationToken);

        return Ok(result);
    }
}
