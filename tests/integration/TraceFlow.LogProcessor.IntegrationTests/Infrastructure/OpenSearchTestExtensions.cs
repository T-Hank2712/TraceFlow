using OpenSearch.Client;
using TraceFlow.LogProcessor.Contracts;

namespace TraceFlow.LogProcessor.IntegrationTests.Infrastructure;

public static class OpenSearchTestExtensions
{
    public static async Task<LogEvent> WaitForLogEventAsync(
        this OpenSearchFixture fixture,
        string eventId,
        TimeSpan timeout)
    {
        var deadline = DateTimeOffset.UtcNow.Add(timeout);

        while (DateTimeOffset.UtcNow < deadline)
        {
            await fixture.Client.Indices.RefreshAsync(fixture.Index);

            var response = await fixture.Client.SearchAsync<LogEvent>(search => search
                .Index(fixture.Index)
                .Query(query => query
                    .Term(term => term
                        .Field("eventId.keyword")
                        .Value(eventId)))
                .Size(1));

            var document = response.Documents.FirstOrDefault();

            if (document is not null)
            {
                return document;
            }

            await Task.Delay(250);
        }

        throw new TimeoutException(
            $"Log event '{eventId}' was not indexed in OpenSearch.");
    }
}