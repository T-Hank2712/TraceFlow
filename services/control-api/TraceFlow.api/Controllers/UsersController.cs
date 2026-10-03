namespace TraceFlow.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Authorize]
[Route("api/v{version:apiVersion}/users")]
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
