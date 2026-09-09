namespace TraceFlow.Api.Application.Projects.Commands.ChangeProjectMemberRole;

public record ChangeProjectMemberRoleResponse(
    Ulid MemberId,
    Ulid ProjectId,
    Ulid UserId,
    string Role,
    string Status,
    DateTime UpdatedAt);