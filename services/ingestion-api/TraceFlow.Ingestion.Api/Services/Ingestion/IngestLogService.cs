using TraceFlow.Ingestion.Api.Clients;
using TraceFlow.Ingestion.Api.Contracts;
using TraceFlow.Ingestion.Api.Contracts.Log;
using TraceFlow.Ingestion.Api.Contracts.BatchLog;
using TraceFlow.Ingestion.Api.Errors;
using TraceFlow.Ingestion.Api.Kafka;
using TraceFlow.Ingestion.Api.Security;
using Microsoft.Extensions.Options;
using FluentValidation;

namespace TraceFlow.Ingestion.Api.Services.Ingestion;

public sealed class IngestLogService : IIngestLogService
{
    private readonly Authenticator _authenticator;
    private readonly EnrichedLogEventFactory _eventFactory;
    private readonly ILogEventPublisher _publisher;
    private readonly IValidator<IngestLogRequest> _logValidator;
    private readonly IValidator<BatchLogRequest> _batchValidator;

    public IngestLogService(
        Authenticator authenticator,
        EnrichedLogEventFactory eventFactory,
        ILogEventPublisher publisher,
        IValidator<IngestLogRequest> logValidator,
        IValidator<BatchLogRequest> batchValidator)
    {
        _authenticator = authenticator;
        _eventFactory = eventFactory;
        _publisher = publisher;
        _logValidator = logValidator;
        _batchValidator = batchValidator;
    }

    public async Task<Result<IngestLogResponse>> IngestAsync(
        IngestLogRequest request,
        string? authorizationHeader,
        CancellationToken cancellationToken)
    {
        var authResult = await _authenticator.AuthenticateAsync(
            authorizationHeader,
            cancellationToken);

        if (!authResult.Success)
        {
            return Result<IngestLogResponse>.Fail(
                authResult.Error!.Code,
                authResult.Error.Message,
                authResult.StatusCode);
        }

        var validationResult = await _logValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Result<IngestLogResponse>.Fail(
                ErrorCodes.ValidateError,
                validationResult.Errors.First().ErrorMessage,
                StatusCodes.Status400BadRequest);
        }

        var tenant = authResult.Data!;
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
            DateTimeOffset.UtcNow
        );
        return Result<IngestLogResponse>.Ok(response);
    }

    public async Task<Result<BatchLogResponse>> BatchLogAsync(
        BatchLogRequest request,
        string? authorizationHeader,
        CancellationToken cancellationToken
    )
    {
        var authResult = await _authenticator.AuthenticateAsync(authorizationHeader, cancellationToken);

        if (!authResult.Success)
        {
            return Result<BatchLogResponse>.Fail(
                authResult.Error!.Code,
                authResult.Error.Message,
                authResult.StatusCode);
        }

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

        var results = new List<BatchLogItemResult>(request.Logs.Count);

        for (int i = 0; i < request.Logs.Count; i++)
        {
            var log = request.Logs[i];

            var validationResult = await _logValidator.ValidateAsync(log, cancellationToken);

            if (!validationResult.IsValid)
            {
                var error = string.Join("; ", validationResult.Errors.Select(x => x.ErrorMessage).Distinct());
                results.Add(
                    new BatchLogItemResult(
                        Index: i,
                        Accepted: false,
                        EventId: null,
                        Error: error));
                continue;
            }
            var eventId = Ulid.NewUlid();
            var logEvent = _eventFactory.Create(log, authResult.Data!, eventId, batchId);

            await _publisher.PublishAsync(logEvent, cancellationToken);
            results.Add(new BatchLogItemResult(i, true, eventId, null));
        }
        var accepted = results.Count(x => x.Accepted);
        var rejected = results.Count(x => !x.Accepted);

        var response = new BatchLogResponse(
            BatchId: batchId,
            Total: request.Logs.Count,
            Accepted: accepted,
            Rejected: rejected,
            Results: results);

        return Result<BatchLogResponse>.Ok(response);
    }
}
