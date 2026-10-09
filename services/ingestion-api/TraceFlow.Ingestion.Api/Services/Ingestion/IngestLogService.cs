namespace TraceFlow.Ingestion.Api.Services.Ingestion;

public sealed class IngestLogService : IIngestLogService
{
    private readonly EnrichedLogEventFactory _eventFactory;
    private readonly ILogEventPublisher _publisher;
    private readonly IValidator<IngestLogRequest> _logValidator;
    private readonly IValidator<BatchLogRequest> _batchValidator;
    private readonly TimeProvider _timeProvider;

    public IngestLogService(
        EnrichedLogEventFactory eventFactory,
        ILogEventPublisher publisher,
        IValidator<IngestLogRequest> logValidator,
        IValidator<BatchLogRequest> batchValidator,
        TimeProvider timeProvider)
    {
        _eventFactory = eventFactory;
        _publisher = publisher;
        _logValidator = logValidator;
        _batchValidator = batchValidator;
        _timeProvider = timeProvider;
    }

    public async Task<Result<IngestLogResponse>> IngestAsync(
        IngestLogRequest request,
        AuthenticatedContext authentication,
        CancellationToken cancellationToken)
    {

        var validationResult = await _logValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Result<IngestLogResponse>.Fail(
                ErrorCodes.ValidateError,
                validationResult.Errors.First().ErrorMessage,
                StatusCodes.Status400BadRequest);
        }

        var tenant = authentication.Tenant;
        var eventId = Ulid.NewUlid();
        var batchId = Ulid.NewUlid();

        var logEvent = _eventFactory.Create(request, tenant, eventId, batchId);

        try
        {
            await _publisher.PublishAsync(logEvent, cancellationToken);
        }
        catch
        {
            return Result<IngestLogResponse>.Fail(ErrorCodes.KafkaPublishFailed, "Failed to publish log event to Kafka.", StatusCodes.Status503ServiceUnavailable);
        }

        var response = new IngestLogResponse(
            eventId,
            true,
            _timeProvider.GetUtcNow()
        );
        return Result<IngestLogResponse>.Ok(response);
    }

    public async Task<Result<BatchLogResponse>> BatchLogAsync(
        BatchLogRequest request,
        AuthenticatedContext authentication,
        CancellationToken cancellationToken)
    {
        var validationBatch = await _batchValidator.ValidateAsync(
            request,
            cancellationToken);

        if (!validationBatch.IsValid)
        {
            return Result<BatchLogResponse>.Fail(
                ErrorCodes.ValidateError,
                validationBatch.Errors.First().ErrorMessage,
                StatusCodes.Status400BadRequest);
        }

        var batchId = Ulid.NewUlid();
        var tenant = authentication.Tenant;
        var totalLogs = request.Logs.Count;

        var results = new BatchLogItemResult[totalLogs];
        var validEvents = new List<(int Index, EnrichedLogEvent Event)>(totalLogs);

        for (var index = 0; index < totalLogs; index++)
        {
            var log = request.Logs[index];
            var validationResult = _logValidator.Validate(log);

            if (!validationResult.IsValid)
            {
                var error = string.Join(
                    "; ",
                    validationResult.Errors
                        .Select(x => x.ErrorMessage)
                        .Distinct());

                results[index] = new BatchLogItemResult(
                    Index: index,
                    Accepted: false,
                    EventId: null,
                    Error: error);

                continue;
            }

            var eventId = Ulid.NewUlid();
            var logEvent = _eventFactory.Create(
                log,
                tenant,
                eventId,
                batchId);

            validEvents.Add((Index: index, Event: logEvent));
        }

        if (validEvents.Count > 0)
        {
            var publishResults = await _publisher.PublishAsync(validEvents, cancellationToken);

            foreach (var publishResult in publishResults)
            {
                results[publishResult.Index] = publishResult;
            }
        }

        var accepted = 0;
        var rejected = 0;

        for (var i = 0; i < totalLogs; i++)
        {
            if (results[i]?.Accepted == true)
            {
                accepted++;
            }
            else
            {
                rejected++;
            }
        }

        var response = new BatchLogResponse(
            BatchId: batchId,
            Total: totalLogs,
            Accepted: accepted,
            Rejected: rejected,
            Results: results);

        return Result<BatchLogResponse>.Ok(response);
    }
}
