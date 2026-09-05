using System.Security.Cryptography.X509Certificates;

namespace TraceFlow.Api.Application.Users.Commands.UpdateProfile;

public record UpdateProfileResponse(
    Ulid Id,
    string Email,
    string UserName,
    string FirstName,
    string LastName,
    string Role,
    string Status
);
