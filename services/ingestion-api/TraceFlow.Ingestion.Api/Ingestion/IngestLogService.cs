using TraceFlow.Ingestion.Api.Clients;
using TraceFlow.Ingestion.Api.Contracts;
using TraceFlow.Ingestion.Api.Contracts.Log;
using TraceFlow.Ingestion.Api.Contracts.BatchLog;
using TraceFlow.Ingestion.Api.Errors;
using TraceFlow.Ingestion.Api.Kafka;
using TraceFlow.Ingestion.Api.Security;

namespace TraceFlow.Ingestion.Api.Ingestion;

public sealed class IngestLogService : IIngestLogService
{
    private readonly Authenticator _authenticator;
    private readonly EnrichedLogEventFactory _eventFactory;
    private readonly ILogEventPublisher _publisher;

    public IngestLogService(
        Authenticator authenticator,
        EnrichedLogEventFactory eventFactory,
        ILogEventPublisher publisher)
    {
        _authenticator = authenticator;
        _eventFactory = eventFactory;
        _publisher = publisher;
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

        var tenant = authResult.Data!;

        var logEvent = _eventFactory.Create(request, tenant);

        try
        {
            await _publisher.PublishAsync(logEvent, cancellationToken);
        }
        catch
        {
            return Result<IngestLogResponse>.Fail(ErrorCodes.KafkaPublishFailed, "Failed to publish log event to Kafka.", StatusCodes.Status503ServiceUnavailable);
        }
        return new Result<IngestLogResponse>(
            true,
            new IngestLogResponse(
                logEvent.EventId,
                true,
                DateTimeOffset.UtcNow),
            null,
            StatusCodes.Status202Accepted);
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

        var tenant = authResult.Data!;

        var logEvents = request.Logs
            .Select(log => _eventFactory.Create(log, tenant))
            .ToList();

        var batchLog = new BatchLogResult(
            Ulid.NewUlid(),
            logEvents);

        try
        {
            await _publisher.PublishAsync(
                logEvents,
                cancellationToken);
        }
        catch
        {
            return Result<BatchLogResponse>.Fail(
                ErrorCodes.KafkaPublishFailed,
                "Failed to publish log events to Kafka.",
                StatusCodes.Status503ServiceUnavailable);
        }
        return new Result<BatchLogResponse>(
            true,
            new BatchLogResponse(
                batchLog.BatchId,
                logEvents.Count,
                DateTimeOffset.UtcNow),
            null,
            StatusCodes.Status202Accepted);
    }

    private static string? ValidateRequest(IngestLogRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message)) return "Message is required.";
        if (string.IsNullOrWhiteSpace(request.Service)) return "Service is required.";
        if (request.Level == LogLevel.None) return "Level is required.";

        var normalizedLevel = request.Level;

        if (normalizedLevel == LogLevel.None)
        {
            return "Level must be one of Trace, Debug, Information, Warning, Error, Critical.";
        }

        return null;
    }
}
