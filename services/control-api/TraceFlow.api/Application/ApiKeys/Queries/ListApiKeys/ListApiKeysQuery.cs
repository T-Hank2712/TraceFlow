namespace TraceFlow.Api.Application.ApiKeys.Queries.ListApiKeys;

public sealed record ListApiKeysQuery(
    Ulid ApplicationId,
    Ulid UserId) : IRequest<IReadOnlyList<ApiKeySummaryResponse>>;
