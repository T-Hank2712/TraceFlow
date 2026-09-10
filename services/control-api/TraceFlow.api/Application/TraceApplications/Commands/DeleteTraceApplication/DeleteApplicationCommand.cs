using MediatR;

namespace TraceFlow.Api.Application.TraceApplications.Commands.DeleteApplication;

public record DeleteApplicationCommand(
    Ulid WorkspaceId,
    Ulid ProjectId,
    Ulid ApplicationId,
    Ulid UserId) : IRequest<DeleteApplicationResponse>;