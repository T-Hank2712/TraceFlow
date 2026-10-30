using OpenSearch.Client;
using Testcontainers.OpenSearch;

namespace TraceFlow.LogProcessor.IntegrationTests.Infrastructure;

public sealed class OpenSearchFixture : IAsyncLifetime
{
    private readonly OpenSearchContainer _container =
        new OpenSearchBuilder("opensearchproject/opensearch:3")
            .WithSecurityEnabled(false)
            .Build();

    public string Url => _container.GetConnectionString();

    public string Username => "admin";

    public string Password => "admin";

    public string Index => "traceflow-logs-test";

    public IOpenSearchClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var settings = new ConnectionSettings(new Uri(Url))
            .DefaultIndex(Index);

        Client = new OpenSearchClient(settings);

        var exists = await Client.Indices.ExistsAsync(Index);

        if (!exists.Exists)
        {
            await Client.Indices.CreateAsync(Index);
        }
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}