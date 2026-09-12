using FluentValidation;

namespace TraceFlow.Api.Application.Logs.Queries.SearchLogs;

public sealed class SearchLogsQueryValidator : AbstractValidator<SearchLogsQuery>
{
    public SearchLogsQueryValidator()
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
            .Must(query => query.From is null || query.To is null || query.From <= query.To)
            .WithMessage("From must be earlier than or equal to To.");
    }
}