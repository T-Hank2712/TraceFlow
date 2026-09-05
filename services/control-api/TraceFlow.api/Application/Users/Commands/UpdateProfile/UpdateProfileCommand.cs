using MediatR;
namespace TraceFlow.Api.Application.Users.Commands.UpdateProfile;
public record UpdateProfileCommand(
    Ulid UserId,
    string? UserName,
    string? FirstName,
    string? LastName
) : IRequest<UpdateProfileResponse>;