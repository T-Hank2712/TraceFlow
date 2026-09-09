using MediatR;

namespace TraceFlow.Api.Application.Projects.Queries.ProjectInvitationInbox;

public record ProjectInvitationInboxQuery(
    Ulid UserId
) : IRequest<IReadOnlyList<ProjectInvitationInboxResponse>>;