using MediatR;

namespace TraceFlow.Api.Application.Users.Queries.GetUser;

public record GetUserQuery(Ulid Id)
    : IRequest<UserResponse?>;

public record UserResponse(
    Ulid Id,
    string Email,
    string UserName,
    string FirstName,
    string LastName,
    DateTime CreatedAt,
    DateTime UpdatedAt
);