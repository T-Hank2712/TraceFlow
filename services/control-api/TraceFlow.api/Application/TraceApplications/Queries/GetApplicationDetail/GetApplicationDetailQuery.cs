namespace TraceFlow.Api.Application.TraceApplications.Queries.GetApplicationDetail;

public record GetApplicationDetailQuery(
    Ulid ApplicationId,
    Ulid UserId
) : IRequest<ApplicationDetailResponse>;
