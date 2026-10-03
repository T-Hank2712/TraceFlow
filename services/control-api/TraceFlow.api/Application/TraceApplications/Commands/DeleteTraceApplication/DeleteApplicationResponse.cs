namespace TraceFlow.Api.Application.TraceApplications.Commands.DeleteTraceApplication;
public record DeleteApplicationResponse(
    Ulid Id,
    Ulid ProjectId,
    string DeleteMode,
    string Message);
