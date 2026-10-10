namespace TraceFlow.Ingestion.Api.Errors;

public static class ErrorCodes
{
    public const string MissingApiKey = "missing_api_key";
    public const string InvalidApiKeyFormat = "invalid_api_key_format";
    public const string InvalidApiKey = "invalid_api_key";
    public const string InvalidPayload = "invalid_payload";
    public const string ControlApiUnavailable = "control_api_unavailable";
    public const string KafkaPublishFailed = "kafka_publish_failed";
    public const string ValidateError = "validate_error";
    public const string RateLimitUnavailable = "rate_limit_unavailable";
    public const string RateLimitExceeded = "rate_limit_exceeded";
    public const string InternalServerError = "internal_server_error";
}
