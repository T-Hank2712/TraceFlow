namespace TraceFlow.Api.Domain.Dtos.Projects;

public record AddProjectMemberRequest(
    string Identifier,
    string Role);