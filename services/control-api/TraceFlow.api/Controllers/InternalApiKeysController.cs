using MediatR;
using Microsoft.AspNetCore.Mvc;
using TraceFlow.Api.Application.ApiKeys.Commands.ValidateApiKey;
using TraceFlow.Api.Domain.Dtos.ApiKeys;

namespace TraceFlow.Api.Controllers;

[ApiController]
[Route("internal/api-keys")]
public class InternalApiKeysController : ControllerBase
{
    private const string InternalSecretHeader = "X-Internal-Secret";
    private readonly ISender _sender;
    private readonly IConfiguration _configuration;

    public InternalApiKeysController(ISender sender, IConfiguration configuration)
    {
        _sender = sender;
        _configuration = configuration;
    }

    [HttpPost("validate")]
    public async Task<IActionResult> ValidateApiKey(
        ValidateApiKeyRequest request,
        CancellationToken cancellationToken)
    {
        var expectedSecret = _configuration["INTERNAL_SERVICE_SECRET"];
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