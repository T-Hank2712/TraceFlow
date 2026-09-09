namespace TraceFlow.Api.Application.TraceApplications.Commands.CreateTraceApplication;

public record CreateTraceApplicationResponse(
    Ulid Id,
    Ulid ProjectId,
    Ulid CreatedByUserId,
    string Name,
    string Slug,
    string? Description,
    string Status,
    DateTime CreatedAt);