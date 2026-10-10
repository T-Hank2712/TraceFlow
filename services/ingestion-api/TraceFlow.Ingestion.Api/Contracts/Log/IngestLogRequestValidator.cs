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
            .Must(level =>
                level is LogLevel.Trace
                    or LogLevel.Debug
                    or LogLevel.Information
                    or LogLevel.Warning
                    or LogLevel.Error
                    or LogLevel.Critical)
            .WithMessage("Log level must be Trace, Debug, Information, Warning, Error, or Critical.");
    }
}
