namespace TraceFlow.Api.Application.TraceApplications.Commands.DeleteTraceApplication;

public record DeleteApplicationCommand(
    Ulid WorkspaceId,
    Ulid ProjectId,
    Ulid ApplicationId,
    Ulid UserId) : IRequest<DeleteApplicationResponse>;
