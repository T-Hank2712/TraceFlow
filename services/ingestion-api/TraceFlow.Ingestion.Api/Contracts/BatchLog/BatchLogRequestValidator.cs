namespace TraceFlow.Ingestion.Api.Contracts.BatchLog;

public sealed class BatchLogRequestValidator
    : AbstractValidator<BatchLogRequest>
{
    public BatchLogRequestValidator(IOptions<IngestionOptions> options)
    {
        var maxBatchSize = options.Value.MaxBatchSize;

        RuleFor(x => x.Logs)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage("Batch logs are required.")
            .NotEmpty()
            .WithMessage("Batch must contain at least one log.")
            .Must(logs => logs.Count <= maxBatchSize)
            .WithMessage($"Batch cannot contain more than {maxBatchSize} logs.");

        RuleForEach(x => x.Logs)
            .NotNull()
            .WithMessage("Batch log item cannot be null.");
    }
}
