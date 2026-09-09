using MediatR;

namespace TraceFlow.Api.Application.Projects.Queries.ProjectInvitationSent;

public record ProjectInvitationSentQuery(
    Ulid WorkspaceId,
    Ulid ProjectId,
    Ulid UserId
) : IRequest<IReadOnlyList<ProjectInvitationSentResponse>>;