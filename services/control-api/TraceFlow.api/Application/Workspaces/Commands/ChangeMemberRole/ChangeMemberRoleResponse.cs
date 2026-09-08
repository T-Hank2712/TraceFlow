namespace TraceFlow.Api.Application.Workspaces.Commands.ChangeMemberRole;

public record ChangeMemberRoleResponse(
    Ulid MemberId,
    Ulid UserId,
    Ulid WorkspaceId,
    string Role,
    string Status,
    DateTime UpdatedAt);