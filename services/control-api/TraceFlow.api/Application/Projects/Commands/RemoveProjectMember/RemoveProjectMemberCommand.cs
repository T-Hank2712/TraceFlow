using MediatR;

namespace TraceFlow.Api.Application.Projects.Commands.RemoveProjectMember;

public record RemoveProjectMemberCommand(
    Ulid WorkspaceId,
    Ulid ProjectId,
    Ulid MemberId,
    Ulid ActorUserId
) : IRequest<RemoveProjectMemberResponse>;