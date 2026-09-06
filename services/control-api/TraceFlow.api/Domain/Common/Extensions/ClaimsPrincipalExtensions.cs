using System.Security.Claims;

namespace TraceFlow.Api.Domain.Common.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Ulid? GetUserId(this ClaimsPrincipal user)
    {
        var userIdValue = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userIdValue) ||
            !Ulid.TryParse(userIdValue, out var userId))
        {
            return null;
        }

        return userId;
    }
}