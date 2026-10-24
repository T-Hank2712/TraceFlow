namespace TraceFlow.Api.Application.TraceApplications.Commands.UpdateApplication;

public record UpdateApplicationCommand(
    Ulid ApplicationId,
    Ulid UserId,
    string? Name,
    string? Slug,
    string? Description
) : IRequest<UpdateApplicationResponse>;
