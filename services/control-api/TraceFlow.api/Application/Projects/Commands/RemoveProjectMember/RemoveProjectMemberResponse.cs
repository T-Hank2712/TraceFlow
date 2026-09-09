namespace TraceFlow.Api.Application.Projects.Commands.RemoveProjectMember;

public record RemoveProjectMemberResponse(
    Ulid RemovedMemberId,
    Ulid ProjectId,
    Ulid RemovedUserId);