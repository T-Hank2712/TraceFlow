using MediatR;

namespace TraceFlow.Api.Application.Workspaces.Queries.InvitationInbox;

public record InvitationInboxQuery(
    Ulid UserId
) : IRequest<IReadOnlyList<InvitationInboxResponse>>;