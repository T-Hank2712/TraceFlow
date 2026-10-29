namespace TraceFlow.Ingestion.Api.Services.Ingestion;

public sealed class IngestLogService : IIngestLogService
{
    private readonly EnrichedLogEventFactory _eventFactory;
    private readonly ILogEventPublisher _publisher;
    private readonly IValidator<IngestLogRequest> _logValidator;
    private readonly IValidator<BatchLogRequest> _batchValidator;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<IngestLogService> _logger;

    public IngestLogService(
        EnrichedLogEventFactory eventFactory,
        ILogEventPublisher publisher,
        IValidator<IngestLogRequest> logValidator,
        IValidator<BatchLogRequest> batchValidator,
        TimeProvider timeProvider,
        ILogger<IngestLogService> logger)
    {
        _eventFactory = eventFactory;
        _publisher = publisher;
        _logValidator = logValidator;
        _batchValidator = batchValidator;
        _timeProvider = timeProvider;
        _logger = logger;
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
                "Request validation failed.",
                StatusCodes.Status400BadRequest,
                ValidationErrorMapper.ToDetails(validationResult.Errors));
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
            return Result<IngestLogResponse>.Fail(
                ErrorCodes.KafkaPublishFailed,
                "Failed to publish log event to Kafka.",
                StatusCodes.Status503ServiceUnavailable,
                ValidationErrorMapper.ToDetails(validationResult.Errors));
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
                StatusCodes.Status400BadRequest,
                ValidationErrorMapper.ToDetails(validationBatch.Errors));
        }

        var batchId = Ulid.NewUlid();
        var tenant = authentication.Tenant;
        var totalLogs = request.Logs.Count;

        var results = new BatchLogItemResult[totalLogs];
        var validEvents = new List<(int Index, EnrichedLogEvent Event)>(totalLogs);

        for (var index = 0; index < totalLogs; index++)
        {
            var log = request.Logs[index];
            if (log is null)
            {
                results[index] = new BatchLogItemResult(
                    Index: index,
                    Accepted: false,
                    EventId: null,
                    Error: "Batch log item cannot be null.");

                continue;
            }

            var validationResult = _logValidator.Validate(log);

            if (!validationResult.IsValid)
            {
                var itemError = string.Join(
                    "; ",
                    validationResult.Errors
                        .Select(x => x.ErrorMessage));

                results[index] = new BatchLogItemResult(
                    Index: index,
                    Accepted: false,
                    EventId: null,
                    Error: itemError);

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

        var kafkaPublishFailed = false;

        if (validEvents.Count > 0)
        {
            try
            {
                var publishResults = await _publisher.PublishAsync(validEvents, cancellationToken);

                foreach (var publishResult in publishResults)
                {
                    results[publishResult.Index] = publishResult;
                }

                foreach (var validEvent in validEvents)
                {
                    if (results[validEvent.Index] is not null)
                    {
                        continue;
                    }

                    results[validEvent.Index] = new BatchLogItemResult(
                        Index: validEvent.Index,
                        Accepted: false,
                        EventId: validEvent.Event.EventId,
                        Error: "Kafka publish did not return a delivery result.");

                    kafkaPublishFailed = true;
                }

                kafkaPublishFailed |= publishResults.Any(result => !result.Accepted);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kafka batch publishing failed. BatchId: {BatchId}", batchId);

                kafkaPublishFailed = true;

                foreach (var validEvent in validEvents)
                {
                    results[validEvent.Index] = new BatchLogItemResult(
                        Index: validEvent.Index,
                        Accepted: false,
                        EventId: validEvent.Event.EventId,
                        Error: "Failed to publish log event to Kafka.");
                }
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

        var error = kafkaPublishFailed
            ? new ErrorResponse(
                ErrorCodes.KafkaPublishFailed,
                "One or more log events could not be published to Kafka.")
            : null;

        var response = new BatchLogResponse(
            BatchId: batchId,
            Total: totalLogs,
            Accepted: accepted,
            Rejected: rejected,
            Results: results,
            Error: error);

        if (kafkaPublishFailed)
        {
            return Result<BatchLogResponse>.Fail(
                response,
                ErrorCodes.KafkaPublishFailed,
                "One or more log events could not be published to Kafka.",
                StatusCodes.Status503ServiceUnavailable);
        }

        return Result<BatchLogResponse>.Ok(response);
    }
}
