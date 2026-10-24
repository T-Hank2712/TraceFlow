namespace TraceFlow.Api.Application.TraceApplications.Commands.DeleteTraceApplication;

public record DeleteApplicationCommand(
    Ulid ApplicationId,
    Ulid UserId) : IRequest<DeleteApplicationResponse>;
