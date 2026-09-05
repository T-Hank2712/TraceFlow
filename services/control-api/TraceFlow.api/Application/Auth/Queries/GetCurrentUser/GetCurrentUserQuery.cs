using MediatR;

namespace TraceFlow.Api.Application.Auth.Queries.GetCurrentUser;
public record GetCurrentUserQuery(
    Ulid UserId
) : IRequest<CurrentUserResponse>;