using MediatR;

namespace TraceFlow.Api.Application.Workspaces.Queries.InvitationSent;

public record InvitationSentQuery(
    Ulid WorkspaceId,
    Ulid UserId
) : IRequest<IReadOnlyList<InvitationSentResponse>>;