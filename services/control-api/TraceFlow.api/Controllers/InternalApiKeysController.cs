namespace TraceFlow.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[RequestBodyLimitPolicy(RequestBodyLimitPolicies.Internal)]
[Route("internal/v{version:apiVersion}/api-keys")]
public class InternalApiKeysController(ISender sender) : ControllerBase
{
    private readonly ISender _sender = sender;
    [Authorize(Policy = InternalServiceAuthentication.Policy)]
    [EnableRateLimiting(RateLimitPolicies.Internal)]
    [HttpPost("validate")]
    public async Task<IActionResult> ValidateApiKey(
        ValidateApiKeyRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new ValidateApiKeyCommand(request.ApiKey),
            cancellationToken);

        return Ok(result);
    }
}
