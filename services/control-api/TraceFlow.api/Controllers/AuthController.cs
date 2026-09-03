using MediatR;
using Microsoft.AspNetCore.Mvc;
using TraceFlow.Api.Application.Users.Commands.CreateUser;
// using TraceFlow.Api.Application.Users.Commands.DeleteUser;
// using TraceFlow.Api.Application.Users.Commands.UpdateUser;

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

    // [HttpGet("{id}")]
    // public async Task<IActionResult> GetById(
    //     Ulid id,
    //     CancellationToken cancellationToken)
    // {
    //     var user = await _sender.Send(
    //         new GetUserQuery(id),
    //         cancellationToken);

    //     if (user is null)
    //     {
    //         return NotFound();
    //     }

    //     return Ok(user);
    // }
}