using MediatR;
using Microsoft.AspNetCore.Mvc;
using TraceFlow.Api.Application.ApiKeys.Commands.ValidateApiKey;
using TraceFlow.Api.Domain.Dtos.ApiKeys;

namespace TraceFlow.Api.Controllers;

[ApiController]
[Route("internal/api-keys")]
public class InternalApiKeysController : ControllerBase
{
    private readonly ISender _sender;

    public InternalApiKeysController(ISender sender)
    {
        _sender = sender;
    }

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