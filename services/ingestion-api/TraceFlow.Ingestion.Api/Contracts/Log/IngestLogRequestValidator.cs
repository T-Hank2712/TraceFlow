namespace TraceFlow.Ingestion.Api.Contracts.Log;

public sealed class IngestLogRequestValidator
    : AbstractValidator<IngestLogRequest>
{
    public IngestLogRequestValidator(IOptions<IngestionOptions> options, TimeProvider timeProvider)
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
        
        RuleFor(x => x.Timestamp)
            .Must(timestamp =>
            {
                if (timestamp is null)
                {
                    return true;
                }

                var utcNow = timeProvider.GetUtcNow();
                var minTimestamp = utcNow.AddDays(-config.MaxPastTimestampDays);
                var maxTimestamp = utcNow.AddMinutes(config.MaxFutureTimestampMinutes);

                return timestamp >= minTimestamp && timestamp <= maxTimestamp;
            })
            .WithMessage(
                $"Timestamp must be within the last {config.MaxPastTimestampDays} days and no more than {config.MaxFutureTimestampMinutes} minutes in the future.");

        RuleFor(x => x.TraceId)
            .MaximumLength(config.MaxTraceIdLength)
            .When(x => !string.IsNullOrWhiteSpace(x.TraceId));

        RuleFor(x => x.CorrelationId)
            .MaximumLength(config.MaxCorrelationIdLength)
            .When(x => !string.IsNullOrWhiteSpace(x.CorrelationId));

        RuleFor(x => x.Metadata)
            .Must(metadata => MetadataValidator.IsValid(metadata, config))
            .WithMessage("Metadata exceeds allowed limits.");
    }
}
