namespace TraceFlow.Ingestion.Api.Clients;

public sealed class ControlApiClient : IApiKeyValidator
{
    private readonly HttpClient _httpClient;
    private readonly ControlApiOptions _options;
    public ControlApiClient(HttpClient httpClient, IOptions<ControlApiOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
    }

    public async Task<ApiKeyValidationResult> ValidateAsync(
        string apiKey,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            _options.ValidateApiPath);

        request.Headers.Add("X-Internal-Secret", _options.InternalServiceSecret);
        request.Content = JsonContent.Create(new { apiKey });

        try
        {
            using var response = await _httpClient.SendAsync(
                request,
                cancellationToken);

            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                return new ApiKeyValidationResult(
                    Valid: false,
                    WorkspaceId: null,
                    ProjectId: null,
                    ApplicationId: null,
                    Environment: null);
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new ControlApiUnavailableException(
                    $"Control API validation failed with status code {(int)response.StatusCode}.");
            }

            var result = await response.Content.ReadFromJsonAsync<ApiKeyValidationResult>(
                cancellationToken);

            return result ?? throw new ControlApiUnavailableException(
                "Control API validation returned an empty response.");
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new ControlApiUnavailableException(
                "Control API validation timed out.",
                ex);
        }
        catch (HttpRequestException ex)
        {
            throw new ControlApiUnavailableException(
                "Control API validation request failed.",
                ex);
        }
    }
}
