using FluentValidation;
using TraceFlow.Ingestion.Api.Configuration;
using Microsoft.Extensions.Options;

namespace TraceFlow.Ingestion.Api.Contracts.BatchLog;

public sealed class BatchLogRequestValidator
    : AbstractValidator<BatchLogRequest>
{
    public BatchLogRequestValidator(IOptions<IngestionOptions> options)
    {
        var maxBatchSize = options.Value.MaxBatchSize;

        RuleFor(x => x.Logs)
            .NotEmpty()
            .WithMessage("Batch must contain at least one log.");

        RuleFor(x => x.Logs)
            .Must(logs => logs.Count <= maxBatchSize)
            .WithMessage($"Batch cannot contain more than {maxBatchSize} logs.");
    }
}
