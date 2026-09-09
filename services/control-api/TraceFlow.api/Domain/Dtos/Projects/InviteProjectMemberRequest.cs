namespace TraceFlow.Api.Domain.Dtos.Projects;

public record InviteProjectMemberRequest(
    string Identifier,
    string Role);