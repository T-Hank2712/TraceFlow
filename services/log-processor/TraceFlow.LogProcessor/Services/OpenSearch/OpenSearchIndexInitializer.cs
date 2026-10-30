namespace TraceFlow.LogProcessor.Services.OpenSearch;

public sealed class OpenSearchIndexInitializer(
    IOptions<OpenSearchOptions> options,
    ILogger<OpenSearchIndexInitializer> logger)
    : IOpenSearchIndexInitializer
{
    private readonly OpenSearchOptions _options = options.Value;
    private readonly ILogger<OpenSearchIndexInitializer> _logger = logger;

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        using var client = CreateClient();

        await EnsureTemplateAsync(client, cancellationToken);

        if (!await IndexExistsAsync(client, cancellationToken))
        {
            await CreateIndexAsync(client, cancellationToken);
        }

        await ValidateIndexMappingAsync(client, cancellationToken);

        _logger.LogInformation(
            "OpenSearch index mapping is ready. Index: {Index}",
            _options.Index);
    }

    private HttpClient CreateClient()
    {
        var handler = new HttpClientHandler();

        if (_options.SkipTlsVerify)
        {
            handler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
        }

        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri(_options.Url.TrimEnd('/') + "/")
        };

        var credentials = Convert.ToBase64String(
            System.Text.Encoding.UTF8.GetBytes(
                $"{_options.Username}:{_options.Password}"));

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Basic",
                credentials);

        return client;
    }

    private async Task EnsureTemplateAsync(
        HttpClient client,
        CancellationToken cancellationToken)
    {
        using var response = await SendJsonAsync(
            client,
            HttpMethod.Put,
            $"_index_template/{Uri.EscapeDataString(GetTemplateName())}",
            new
            {
                index_patterns = new[] { _options.Index },
                priority = 500,
                template = new
                {
                    mappings = CreateMappings()
                }
            },
            cancellationToken);

        await EnsureSuccessAsync(response, "create OpenSearch index template", cancellationToken);
    }

    private async Task<bool> IndexExistsAsync(
        HttpClient client,
        CancellationToken cancellationToken)
    {
        using var response = await client.SendAsync(
            new HttpRequestMessage(
                HttpMethod.Head,
                Uri.EscapeDataString(_options.Index)),
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        await EnsureSuccessAsync(response, "check OpenSearch index", cancellationToken);
        return true;
    }

    private async Task CreateIndexAsync(
        HttpClient client,
        CancellationToken cancellationToken)
    {
        using var response = await SendJsonAsync(
            client,
            HttpMethod.Put,
            Uri.EscapeDataString(_options.Index),
            new
            {
                mappings = CreateMappings()
            },
            cancellationToken);

        if (response.IsSuccessStatusCode ||
            response.StatusCode == HttpStatusCode.BadRequest)
        {
            return;
        }

        await EnsureSuccessAsync(response, "create OpenSearch index", cancellationToken);
    }

    private async Task ValidateIndexMappingAsync(
        HttpClient client,
        CancellationToken cancellationToken)
    {
        using var response = await client.GetAsync(
            $"{Uri.EscapeDataString(_options.Index)}/_mapping",
            cancellationToken);

        await EnsureSuccessAsync(response, "read OpenSearch index mapping", cancellationToken);

        using var document = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync(cancellationToken));

        var indexMapping = document.RootElement
            .EnumerateObject()
            .FirstOrDefault()
            .Value;

        if (indexMapping.ValueKind is JsonValueKind.Undefined ||
            !indexMapping.TryGetProperty("mappings", out var mappings) ||
            !mappings.TryGetProperty("dynamic", out var dynamic) ||
            !IsDynamicMappingDisabled(dynamic) ||
            !mappings.TryGetProperty("properties", out var properties) ||
            !properties.TryGetProperty("metadata", out var metadata) ||
            !metadata.TryGetProperty("type", out var type) ||
            !string.Equals(type.GetString(), "flat_object", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"OpenSearch index '{_options.Index}' does not use the required safe metadata mapping. " +
                "Create a new index with the current template, reindex the data, and update OpenSearch__Index.");
        }
    }

    private static bool IsDynamicMappingDisabled(JsonElement dynamic)
        => dynamic.ValueKind == JsonValueKind.False ||
           dynamic.ValueKind == JsonValueKind.String &&
           string.Equals(dynamic.GetString(), "false", StringComparison.OrdinalIgnoreCase);

    private async Task<HttpResponseMessage> SendJsonAsync(
        HttpClient client,
        HttpMethod method,
        string path,
        object body,
        CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(method, path)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(body),
                System.Text.Encoding.UTF8,
                "application/json")
        };

        return await client.SendAsync(request, cancellationToken);
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        string operation,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        throw new InvalidOperationException(
            $"Failed to {operation}. Status: {(int)response.StatusCode}. Response: {content}");
    }

    private object CreateMappings()
    {
        return new
        {
            dynamic = false,
            properties = new Dictionary<string, object>
            {
                ["eventId"] = KeywordField(),
                ["workspaceId"] = KeywordField(),
                ["projectId"] = KeywordField(),
                ["applicationId"] = KeywordField(),
                ["environment"] = KeywordField(),
                ["service"] = KeywordField(),
                ["level"] = new { type = "integer" },
                ["message"] = new { type = "text" },
                ["timestamp"] = new { type = "date" },
                ["traceId"] = KeywordField(),
                ["correlationId"] = KeywordField(),
                ["metadata"] = new { type = "flat_object" }
            }
        };
    }

    private static object KeywordField()
    {
        return new { type = "keyword" };
    }

    private string GetTemplateName()
    {
        return $"{_options.Index}-mapping";
    }
}
