namespace TraceFlow.Api.Application.TraceLogs.Queries.SearchLogs;

public sealed class SearchLogsQueryValidator : AbstractValidator<SearchLogsQuery>
{
    private static readonly TimeSpan MaxSearchRange = TimeSpan.FromDays(31);
    public SearchLogsQueryValidator(TimeProvider timeProvider)
    {
        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(query => query.Level)
            .MaximumLength(20)
            .When(query => query.Level is not null);

        RuleFor(query => query.Service)
            .MaximumLength(100)
            .When(query => query.Service is not null);

        RuleFor(query => query.Environment)
            .MaximumLength(50)
            .When(query => query.Environment is not null);

        RuleFor(query => query.TraceId)
            .MaximumLength(100)
            .When(query => query.TraceId is not null);

        RuleFor(query => query.CorrelationId)
            .MaximumLength(100)
            .When(query => query.CorrelationId is not null);

        RuleFor(query => query)
            .Must(query => IsValidTimeRange(query, timeProvider.GetUtcNow()))
            .WithMessage("From must be earlier than or equal to To.");
    }
    private static bool IsValidTimeRange(
        SearchLogsQuery query,
        DateTimeOffset utcNow)
    {
        var to = query.To?.ToUniversalTime() ?? utcNow;
        var from = query.From?.ToUniversalTime() ?? to.AddHours(-24);

        return from <= to && to - from <= MaxSearchRange;
    }
}
