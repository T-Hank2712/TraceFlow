using MediatR;

namespace TraceFlow.Api.Application.TraceApplications.Commands.CreateTraceApplication;

public record CreateTraceApplicationCommand(
    Ulid WorkspaceId,
    Ulid ProjectId,
    Ulid UserId,
    string Name,
    string Slug,
    string? Description
) : IRequest<CreateTraceApplicationResponse>;