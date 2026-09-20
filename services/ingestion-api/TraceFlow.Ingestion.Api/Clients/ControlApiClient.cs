
using Microsoft.Extensions.Options;
using TraceFlow.Ingestion.Api.Configuration;

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
    }

    public async Task<ApiKeyValidationResult> ValidateAsync(string apiKey, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, _options.ValidateApiPath);
        request.Headers.Add("X-Internal-Secret", _options.InternalServiceSecret);
        request.Content = JsonContent.Create(new { apiKey });

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return new(false, null, null, null, null);
        }

        return await response.Content.ReadFromJsonAsync<ApiKeyValidationResult>(cancellationToken)
            ?? new(false, null, null, null, null);
    }
}
