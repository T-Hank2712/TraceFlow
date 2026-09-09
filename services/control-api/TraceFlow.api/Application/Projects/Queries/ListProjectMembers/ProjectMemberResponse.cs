namespace TraceFlow.Api.Application.Projects.Queries.ListProjectMembers;

public record ProjectMemberResponse(
    Ulid MemberId,
    Ulid ProjectId,
    Ulid UserId,
    string UserName,
    string Email,
    string Role,
    string Status,
    DateTime JoinedAt);