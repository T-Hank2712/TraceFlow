using FluentValidation;
using TraceFlow.Ingestion.Api.Configuration;
using Microsoft.Extensions.Options;

namespace TraceFlow.Ingestion.Api.Contracts.Log;

public sealed class IngestLogRequestValidator
    : AbstractValidator<IngestLogRequest>
{
    public IngestLogRequestValidator(IOptions<IngestionOptions> options)
    {
        var config = options.Value;

        RuleFor(x => x.Service)
            .NotEmpty()
            .MaximumLength(config.MaxServiceLength);

        RuleFor(x => x.Message)
            .NotEmpty()
            .MaximumLength(config.MaxMessageLength);

        RuleFor(x => x.Level)
            .NotEqual(LogLevel.None);
    }
}
