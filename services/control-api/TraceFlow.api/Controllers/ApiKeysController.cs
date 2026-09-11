using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TraceFlow.Api.Application.ApiKeys.Commands.CreateApiKey;
using TraceFlow.Api.Domain.Common.Extensions;
using TraceFlow.Api.Domain.Dtos.ApiKeys;
using TraceFlow.Api.Application.ApiKeys.Queries.ListApiKeys;
using TraceFlow.Api.Application.ApiKeys.Commands.RevokeApiKey;

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