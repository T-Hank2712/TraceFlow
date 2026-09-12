using System.Text;
using System.Text.Json;
using TraceFlow.Api.Application.Common.Logs;
using TraceFlow.Api.Domain.Dtos.Logs;
using TraceFlow.Api.Application.Logs.Queries.SearchLogs;
using TraceFlow.Api.Application.Common.Exceptions;

namespace TraceFlow.Api.Infrastructure.OpenSearch;

public sealed class OpenSearchLogSearchReader : ILogSearchReader
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public OpenSearchLogSearchReader(
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<SearchLogsResponse> SearchAsync(
        SearchLogsQuery query,
        CancellationToken cancellationToken)
    {
        var index = _configuration["OpenSearch:Index"]
            ?? throw new InvalidOperationException("OpenSearch index is not configured.");

        var filters = BuildFilters(query);

        var payload = new
        {
            from = (query.Page - 1) * query.PageSize,
            size = query.PageSize,
            sort = new object[]
            {
                new Dictionary<string, object>
                {
                    ["timestamp"] = new
                    {
                        order = "desc"
                    }
                }
            },
            query = new
            {
                @bool = new
                {
                    filter = filters
                }
            }
        };

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{index}/_search");

        request.Content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        try
        {
            using var response = await _httpClient.SendAsync(
                request,
                cancellationToken);

            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new ExternalServiceException(
                    "Log search backend is unavailable.");
            }

            return ParseResponse(content, query.Page, query.PageSize);
        }
        catch (ExternalServiceException)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            throw new ExternalServiceException(
                "Log search backend is unavailable.",
                ex);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new ExternalServiceException(
                "Log search backend timed out.",
                ex);
        }
        catch (JsonException ex)
        {
            throw new ExternalServiceException(
                "Log search backend returned an invalid response.",
                ex);
        }
    }

    private static List<object> BuildFilters(SearchLogsQuery query)
    {
        var filters = new List<object>
        {
            Term("workspaceId.keyword", query.WorkspaceId.ToString()),
            Term("projectId.keyword", query.ProjectId.ToString())
        };

        if (query.ApplicationId is not null)
        {
            filters.Add(Term("applicationId.keyword", query.ApplicationId.Value.ToString()));
        }

        AddTermIfPresent(filters, "environment.keyword", query.Environment?.ToLowerInvariant());
        AddTermIfPresent(filters, "level.keyword", query.Level?.ToUpperInvariant());
        AddTermIfPresent(filters, "service.keyword", query.Service);
        AddTermIfPresent(filters, "traceId.keyword", query.TraceId);
        AddTermIfPresent(filters, "correlationId.keyword", query.CorrelationId);

        if (query.From is not null || query.To is not null)
        {
            var range = new Dictionary<string, object>();

            if (query.From is not null)
            {
                range["gte"] = query.From.Value.ToUniversalTime();
            }

            if (query.To is not null)
            {
                range["lte"] = query.To.Value.ToUniversalTime();
            }

            filters.Add(new
            {
                range = new Dictionary<string, object>
                {
                    ["timestamp"] = range
                }
            });
        }

        return filters;
    }

    private static object Term(string field, string value)
    {
        return new
        {
            term = new Dictionary<string, object>
            {
                [field] = value
            }
        };
    }

    private static void AddTermIfPresent(
        List<object> filters,
        string field,
        string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            filters.Add(Term(field, value.Trim()));
        }
    }

    private static SearchLogsResponse ParseResponse(
        string content,
        int page,
        int pageSize)
    {
        using var document = JsonDocument.Parse(content);

        var hitsRoot = document.RootElement.GetProperty("hits");

        var total = hitsRoot
            .GetProperty("total")
            .GetProperty("value")
            .GetInt64();

        var items = new List<LogSearchItemResponse>();

        foreach (var hit in hitsRoot.GetProperty("hits").EnumerateArray())
        {
            var source = hit.GetProperty("_source");

            var log = JsonSerializer.Deserialize<OpenSearchLogDocument>(
                source.GetRawText());

            if (log is null)
            {
                continue;
            }

            items.Add(new LogSearchItemResponse(
                log.EventId,
                log.Timestamp,
                log.ReceivedAt,
                log.WorkspaceId,
                log.ProjectId,
                log.ApplicationId,
                log.Environment,
                log.Service,
                log.Level,
                log.Message,
                log.TraceId,
                log.CorrelationId,
                log.Metadata));
        }

        return new SearchLogsResponse(items, total, page, pageSize);
    }
}