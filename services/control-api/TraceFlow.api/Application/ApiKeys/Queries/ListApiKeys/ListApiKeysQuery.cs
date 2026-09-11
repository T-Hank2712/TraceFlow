using MediatR;

namespace TraceFlow.Api.Application.ApiKeys.Queries.ListApiKeys;

public sealed record ListApiKeysQuery(
    Ulid WorkspaceId,
    Ulid ProjectId,
    Ulid ApplicationId,
    Ulid UserId) : IRequest<IReadOnlyList<ApiKeySummaryResponse>>;