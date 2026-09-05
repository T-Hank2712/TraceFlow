using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TraceFlow.Api.Domain.Dtos;
using TraceFlow.Api.Application.Users.Commands.UpdateProfile;

namespace TraceFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly ISender _sender;

    public UsersController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPatch("me/profile")]
    public async Task<IActionResult> UpdateMyProfile(
        UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userIdValue) ||
            !Ulid.TryParse(userIdValue, out var userId))
        {
            return Unauthorized();
        }

        var result = await _sender.Send(
            new UpdateProfileCommand(
                userId,
                request.UserName,
                request.FirstName,
                request.LastName),
            cancellationToken);

        return Ok(result);
    }
}