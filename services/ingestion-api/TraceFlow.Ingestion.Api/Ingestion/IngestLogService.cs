using TraceFlow.Ingestion.Api.Clients;
using TraceFlow.Ingestion.Api.Contracts;
using TraceFlow.Ingestion.Api.Errors;
using TraceFlow.Ingestion.Api.Kafka;

namespace TraceFlow.Ingestion.Api.Ingestion;

public sealed class IngestLogService : IIngestLogService
{
    private readonly ApiKeyHeaderParser _apiKeyParser;
    private readonly IApiKeyValidator _apiKeyValidator;
    private readonly EnrichedLogEventFactory _eventFactory;
    private readonly ILogEventPublisher _publisher;

    public IngestLogService(
        ApiKeyHeaderParser apiKeyParser,
        IApiKeyValidator apiKeyValidator,
        EnrichedLogEventFactory eventFactory,
        ILogEventPublisher publisher)
    {
        _apiKeyParser = apiKeyParser;
        _apiKeyValidator = apiKeyValidator;
        _eventFactory = eventFactory;
        _publisher = publisher;
    }

    public async Task<IngestLogResult> IngestAsync(
        IngestLogRequest request,
        string? authorizationHeader,
        CancellationToken cancellationToken)
    {
        var parsedKey = _apiKeyParser.Parse(authorizationHeader);

        if (!parsedKey.Success)
        {
            return Fail(parsedKey.ErrorCode!, parsedKey.ErrorMessage!, StatusCodes.Status401Unauthorized);
        }

        var validationError = ValidateRequest(request);

        if (validationError is not null)
        {
            return Fail(ErrorCodes.InvalidPayload, validationError, StatusCodes.Status400BadRequest);
        }

        var tenant = await _apiKeyValidator.ValidateAsync(parsedKey.ApiKey!, cancellationToken);

        if (!tenant.Valid)
        {
            return Fail(ErrorCodes.InvalidApiKey, "API key is invalid, revoked, or expired.", StatusCodes.Status401Unauthorized);
        }

        var logEvent = _eventFactory.Create(request, tenant);

        try
        {
            await _publisher.PublishAsync(logEvent, cancellationToken);
        }
        catch
        {
            return Fail(ErrorCodes.KafkaPublishFailed, "Failed to publish log event to Kafka.", StatusCodes.Status503ServiceUnavailable);
        }

        return new IngestLogResult(
            true,
            new IngestLogResponse(logEvent.EventId, "accepted", DateTimeOffset.UtcNow),
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

    private static IngestLogResult Fail(string code, string message, int statusCode)
    {
        return new IngestLogResult(false, null, new ErrorResponse(code, message), statusCode);
    }
}