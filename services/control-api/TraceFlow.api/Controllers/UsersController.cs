using MediatR;
using Microsoft.AspNetCore.Mvc;
using TraceFlow.Api.Application.Users.Commands.CreateUser;
// using TraceFlow.Api.Application.Users.Commands.DeleteUser;
// using TraceFlow.Api.Application.Users.Commands.UpdateUser;
using TraceFlow.Api.Application.Users.Queries.GetUser;

namespace TraceFlow.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly ISender _sender;

    public UsersController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Create(
        CreateUserCommand command,
        CancellationToken cancellationToken)
    {
        var userId = await _sender.Send(
            command,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = userId },
            new { id = userId });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(
        Ulid id,
        CancellationToken cancellationToken)
    {
        var user = await _sender.Send(
            new GetUserQuery(id),
            cancellationToken);

        if (user is null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    // [HttpPut("{id:guid}")]
    // public async Task<IActionResult> Update(
    //     Guid id,
    //     UpdateUserRequest request,
    //     CancellationToken cancellationToken)
    // {
    //     await _sender.Send(
    //         new UpdateUserCommand(
    //             id,
    //             request.DisplayName),
    //         cancellationToken);

    //     return NoContent();
    // }

    // [HttpDelete("{id:guid}")]
    // public async Task<IActionResult> Delete(
    //     Guid id,
    //     CancellationToken cancellationToken)
    // {
    //     await _sender.Send(
    //         new DeleteUserCommand(id),
    //         cancellationToken);

    //     return NoContent();
    // }
}