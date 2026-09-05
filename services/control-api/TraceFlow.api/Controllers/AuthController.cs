using MediatR;
using Microsoft.AspNetCore.Mvc;
using TraceFlow.Api.Application.Auth.Commands.Register;
using TraceFlow.Api.Application.Auth.Commands.Login;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using TraceFlow.Api.Application.Auth.Queries.GetCurrentUser;
using TraceFlow.Api.Application.Auth.Commands.RefreshSession;

namespace TraceFlow.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        RegisterCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Register), new { email = result.Email }, result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(
        CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userIdValue) ||
            !Ulid.TryParse(userIdValue, out var userId))
        {
            return Unauthorized();
        }

        var result = await _sender.Send(
            new GetCurrentUserQuery(userId),
            cancellationToken);

        return Ok(result);
    }
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken(RefreshSessionCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }
}