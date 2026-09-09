namespace TraceFlow.Api.Application.Workspaces.Commands.RemoveMember;

public record RemoveMemberResponse(
    Ulid RemovedMemberId,
    Ulid WorkspaceId,
    Ulid RemovedUserId);