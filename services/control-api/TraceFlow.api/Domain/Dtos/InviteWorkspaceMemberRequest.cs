namespace TraceFlow.Api.Domain.Dtos;

public record InviteWorkspaceMemberRequest(
    string Identifier,
    string Role);