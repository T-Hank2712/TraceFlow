namespace TraceFlow.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Authorize]
[Route("api/v{version:apiVersion}/applications/{applicationId}/api-keys")]
public class ApiKeysController(ISender sender) : ControllerBase
{
    private readonly ISender _sender = sender;

    [HttpPost]
    public async Task<IActionResult> CreateApiKey(
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
                applicationId,
                userId.Value),
            cancellationToken);

        return Ok(result);
    }
    [HttpDelete("~/api/v{version:apiVersion}/api-keys/{apiKeyId}")]
    public async Task<IActionResult> RevokeApiKey(
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
                apiKeyId,
                userId.Value),
            cancellationToken);

        return Ok(result);
    }
}
