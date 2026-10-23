namespace TraceFlow.Api.Application.Projects.Commands.RemoveProjectMember;

public record RemoveProjectMemberCommand(
    Ulid MemberId,
    Ulid ActorUserId
) : IRequest<RemoveProjectMemberResponse>;
