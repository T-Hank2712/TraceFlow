namespace TraceFlow.Api.Application.Projects.Queries.GetProjectById;

public record GetProjectByIdQuery(
    Ulid ProjectId,
    Ulid UserId
) : IRequest<ProjectDetailResponse>;
