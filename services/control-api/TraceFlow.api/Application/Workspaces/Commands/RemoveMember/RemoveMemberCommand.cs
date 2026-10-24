namespace TraceFlow.Api.Application.Workspaces.Commands.RemoveMember;

public record RemoveMemberCommand(
    Ulid MemberId,
    Ulid ActorUserId
) : IRequest<RemoveMemberResponse>;
