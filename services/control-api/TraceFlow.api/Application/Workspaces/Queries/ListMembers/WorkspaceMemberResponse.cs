namespace TraceFlow.Api.Application.Workspaces.Queries.ListMembers;

public record WorkspaceMemberResponse(
    Ulid MemberId,
    Ulid UserId,
    string UserName,
    string Email,
    string Role,
    string Status,
    DateTime JoinedAt);