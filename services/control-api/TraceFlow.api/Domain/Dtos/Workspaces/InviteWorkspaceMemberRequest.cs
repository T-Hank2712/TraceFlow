namespace TraceFlow.Api.Domain.Dtos.Workspaces;

public record InviteWorkspaceMemberRequest(
    string Identifier,
    string Role);