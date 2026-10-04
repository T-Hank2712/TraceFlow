namespace TraceFlow.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("internal/v{version:apiVersion}/api-keys")]
public class InternalApiKeysController(ISender sender, IOptions<InternalServiceOptions> options) : ControllerBase
{
    private readonly ISender _sender = sender;
    private readonly InternalServiceOptions _options = options.Value;

    private const string InternalSecretHeader = "X-Internal-Secret";
 [HttpPost("validate")]
    public async Task<IActionResult> ValidateApiKey(
        ValidateApiKeyRequest request,
        CancellationToken cancellationToken)
    {
        var expectedSecret = _options.Secret;
        var providedSecret = Request.Headers[InternalSecretHeader].ToString();

        if (string.IsNullOrWhiteSpace(expectedSecret) || string.IsNullOrWhiteSpace(providedSecret) || providedSecret != expectedSecret)
        {
            return Unauthorized();
        }

        var result = await _sender.Send(
            new ValidateApiKeyCommand(request.ApiKey),
            cancellationToken);

        return Ok(result);
    }
}
