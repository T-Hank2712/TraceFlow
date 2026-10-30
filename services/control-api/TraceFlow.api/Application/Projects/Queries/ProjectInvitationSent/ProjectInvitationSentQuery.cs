namespace TraceFlow.Api.Application.Projects.Queries.ProjectInvitationSent;

public record ProjectInvitationSentQuery(
    Ulid ProjectId,
    Ulid UserId
) : IRequest<IReadOnlyList<ProjectInvitationSentResponse>>;
