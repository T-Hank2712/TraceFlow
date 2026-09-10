using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TraceFlow.Api.Application.ApiKeys.Commands.CreateApiKey;
using TraceFlow.Api.Domain.Common.Extensions;
using TraceFlow.Api.Domain.Dtos.ApiKeys;

namespace TraceFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/workspaces/{workspaceId}/projects/{projectId}/applications/{applicationId}/api-keys")]
public class ApiKeysController : ControllerBase
{
    private readonly ISender _sender;

    public ApiKeysController(ISender sender)
    {
        _sender = sender;
    }

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
}