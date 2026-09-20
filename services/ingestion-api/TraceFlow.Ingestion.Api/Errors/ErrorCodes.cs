namespace TraceFlow.Ingestion.Api.Errors;

public static class ErrorCodes
{
    public const string MissingApiKey = "missing_api_key";
    public const string InvalidApiKeyFormat = "invalid_api_key_format";
    public const string InvalidApiKey = "invalid_api_key";
    public const string InvalidPayload = "invalid_payload";
    public const string ControlApiUnavailable = "control_api_unavailable";
    public const string KafkaPublishFailed = "kafka_publish_failed";
}
