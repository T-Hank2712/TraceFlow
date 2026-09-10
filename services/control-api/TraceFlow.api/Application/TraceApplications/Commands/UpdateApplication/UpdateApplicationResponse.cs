namespace TraceFlow.Api.Application.TraceApplications.Commands.UpdateApplication;

public record UpdateApplicationResponse(
    Ulid Id,
    Ulid ProjectId,
    string Name,
    string Slug,
    string? Description,
    string Status,
    DateTime UpdatedAt);