using MediatR;

namespace TraceFlow.Api.Application.Workspaces.Commands.RemoveMember;

public record RemoveMemberCommand(
    Ulid WorkspaceId,
    Ulid MemberId,
    Ulid ActorUserId
) : IRequest<RemoveMemberResponse>;