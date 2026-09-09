using MediatR;

namespace TraceFlow.Api.Application.TraceApplications.Queries.GetApplicationDetail;

public record GetApplicationDetailQuery(
    Ulid WorkspaceId,
    Ulid ProjectId,
    Ulid ApplicationId,
    Ulid UserId
) : IRequest<ApplicationDetailResponse>;