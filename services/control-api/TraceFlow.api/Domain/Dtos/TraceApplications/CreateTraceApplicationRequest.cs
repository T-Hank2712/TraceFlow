namespace TraceFlow.Api.Domain.Dtos.TraceApplications;

public record CreateTraceApplicationRequest(
    string Name,
    string Slug,
    string? Description);