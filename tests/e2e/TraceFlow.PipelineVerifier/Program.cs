using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Text;
using System.Text.Json;
using Confluent.Kafka;

var options = VerifierOptions.FromEnvironment();

using var timeout = new CancellationTokenSource(
    TimeSpan.FromSeconds(options.TimeoutSeconds));
using var dlqCollector = new DlqCollector(options, timeout.Token);

dlqCollector.Start();

using var openSearch = CreateOpenSearchClient(options);

var deadline = DateTimeOffset.UtcNow.AddSeconds(options.TimeoutSeconds);
OpenSearchEvents indexedEvents = OpenSearchEvents.Empty;
E2eWorkload? workload = null;

while (DateTimeOffset.UtcNow < deadline)
{
    indexedEvents = await ReadOpenSearchEventsAsync(
        openSearch,
        options,
        CancellationToken.None);

    workload ??= indexedEvents.Workload ?? dlqCollector.Workload;

    if (workload is not null)
    {
        var observedCount = indexedEvents.EventKeys.Count + dlqCollector.EventKeys.Count;

        if (observedCount >= workload.ExpectedEventCount)
        {
            break;
        }
    }

    await Task.Delay(TimeSpan.FromSeconds(1));
}

indexedEvents = await ReadOpenSearchEventsAsync(
    openSearch,
    options,
    CancellationToken.None);

await dlqCollector.StopAsync();

workload ??= indexedEvents.Workload ?? dlqCollector.Workload;

if (workload is null)
{
    throw new InvalidOperationException(
        "No E2E workload metadata was found in OpenSearch or DLQ for this run.");
}

var expectedKeys = CreateExpectedKeys(options.RunId, workload);

var indexedKeys = indexedEvents.EventKeys;
var dlqKeys = dlqCollector.EventKeys;
var terminalKeys = indexedKeys.Union(dlqKeys).ToHashSet(StringComparer.Ordinal);
var missingKeys = expectedKeys.Except(terminalKeys).ToArray();
var unexpectedKeys = terminalKeys.Except(expectedKeys).ToArray();
var indexedAndDlq = indexedKeys.Intersect(dlqKeys).ToArray();

Console.WriteLine($"E2E run ID: {options.RunId}");
Console.WriteLine($"Expected events: {expectedKeys.Count}");
Console.WriteLine($"Indexed events: {indexedKeys.Count}");
Console.WriteLine($"DLQ events: {dlqKeys.Count}");
Console.WriteLine($"Missing events: {missingKeys.Length}");
Console.WriteLine($"Unexpected events: {unexpectedKeys.Length}");
Console.WriteLine($"Indexed and DLQ: {indexedAndDlq.Length}");
Console.WriteLine($"Duplicate OpenSearch event keys: {indexedEvents.DuplicateEventKeys}");
Console.WriteLine($"Duplicate DLQ event keys: {dlqCollector.DuplicateEventKeys}");

if (missingKeys.Length == 0 &&
    unexpectedKeys.Length == 0 &&
    indexedAndDlq.Length == 0 &&
    indexedEvents.DuplicateEventKeys == 0 &&
    dlqCollector.DuplicateEventKeys == 0)
{
    Console.WriteLine("E2E reconciliation: PASS");
    return;
}

PrintSample("Missing", missingKeys);
PrintSample("Unexpected", unexpectedKeys);
PrintSample("Indexed and DLQ", indexedAndDlq);

throw new InvalidOperationException(
    "E2E reconciliation failed. Every generated event key must exist exactly once in OpenSearch or DLQ.");

static HashSet<string> CreateExpectedKeys(string runId, E2eWorkload workload)
{
    var expected = new HashSet<string>(StringComparer.Ordinal);

    for (var vu = 1; vu <= workload.VirtualUsers; vu++)
    {
        for (var iteration = 0; iteration < workload.IterationsPerVu; iteration++)
        {
            expected.Add($"{runId}-{vu}-{iteration}");
        }
    }

    return expected;
}

static HttpClient CreateOpenSearchClient(VerifierOptions options)
{
    var handler = new SocketsHttpHandler();

    if (options.SkipTlsVerify)
    {
        handler.SslOptions = new SslClientAuthenticationOptions
        {
            RemoteCertificateValidationCallback =
                (_, _, _, _) => true
        };
    }

    var client = new HttpClient(handler)
    {
        BaseAddress = new Uri(options.OpenSearchUrl.TrimEnd('/') + "/"),
        Timeout = TimeSpan.FromSeconds(30)
    };

    var credentials = Convert.ToBase64String(
        Encoding.UTF8.GetBytes($"{options.OpenSearchUsername}:{options.OpenSearchPassword}"));

    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Basic", credentials);

    return client;
}

static async Task<OpenSearchEvents> ReadOpenSearchEventsAsync(
    HttpClient client,
    VerifierOptions options,
    CancellationToken cancellationToken)
{
    const int pageSize = 1_000;
    string? scrollId = null;
    var eventKeys = new HashSet<string>(StringComparer.Ordinal);
    var duplicateEventKeys = 0;
    E2eWorkload? workload = null;

    try
    {
        var query = JsonSerializer.Serialize(new
        {
            size = pageSize,
            sort = new[] { "_doc" },
            query = new
            {
                term = new Dictionary<string, string>
                {
                    ["metadata.e2eRunId"] = options.RunId
                }
            }
        });

        using var searchResponse = await client.PostAsync(
            $"{options.OpenSearchIndex}/_search?scroll=1m",
            new StringContent(query, Encoding.UTF8, "application/json"),
            cancellationToken);

        if (searchResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return OpenSearchEvents.Empty;
        }

        searchResponse.EnsureSuccessStatusCode();

        var page = await ReadSearchPageAsync(
            searchResponse,
            cancellationToken);

        scrollId = page.ScrollId;

        while (page.EventKeys.Count > 0)
        {
            workload ??= page.Workload;

            if (page.Workload is not null && workload != page.Workload)
            {
                throw new InvalidOperationException(
                    "OpenSearch returned inconsistent E2E workload metadata for the same run.");
            }

            foreach (var eventKey in page.EventKeys)
            {
                if (!eventKeys.Add(eventKey))
                {
                    duplicateEventKeys++;
                }
            }

            var scrollRequest = JsonSerializer.Serialize(new
            {
                scroll = "1m",
                scroll_id = scrollId
            });

            using var scrollResponse = await client.PostAsync(
                "_search/scroll",
                new StringContent(scrollRequest, Encoding.UTF8, "application/json"),
                cancellationToken);

            scrollResponse.EnsureSuccessStatusCode();

            page = await ReadSearchPageAsync(scrollResponse, cancellationToken);
            scrollId = page.ScrollId;
        }

        return new OpenSearchEvents(eventKeys, duplicateEventKeys, workload);
    }
    finally
    {
        if (!string.IsNullOrWhiteSpace(scrollId))
        {
            using var clearRequest = new HttpRequestMessage(
                HttpMethod.Delete,
                "_search/scroll")
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(new { scroll_id = new[] { scrollId } }),
                    Encoding.UTF8,
                    "application/json")
            };

            using var clearResponse = await client.SendAsync(
                clearRequest,
                CancellationToken.None);
        }
    }
}

static async Task<SearchPage> ReadSearchPageAsync(
    HttpResponseMessage response,
    CancellationToken cancellationToken)
{
    await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
    using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

    var root = document.RootElement;
    var scrollId = root.TryGetProperty("_scroll_id", out var scrollIdElement)
        ? scrollIdElement.GetString()
        : null;

    var eventKeys = new List<string>();
    E2eWorkload? workload = null;

    foreach (var hit in root
                 .GetProperty("hits")
                 .GetProperty("hits")
                 .EnumerateArray())
    {
        if (!hit.TryGetProperty("_source", out var source) ||
            !source.TryGetProperty("metadata", out var metadata) ||
            !metadata.TryGetProperty("e2eEventKey", out var eventKey))
        {
            continue;
        }

        var value = eventKey.GetString();

        if (!string.IsNullOrWhiteSpace(value))
        {
            eventKeys.Add(value);
        }

        if (metadata.TryGetProperty("e2eExpectedVus", out var virtualUsers) &&
            metadata.TryGetProperty("e2eExpectedIterationsPerVu", out var iterationsPerVu) &&
            E2eValueParser.TryGetPositiveInteger(virtualUsers, out var vus) &&
            E2eValueParser.TryGetPositiveInteger(iterationsPerVu, out var iterations))
        {
            var candidate = new E2eWorkload(vus, iterations);

            if (workload is not null && workload != candidate)
            {
                throw new InvalidOperationException(
                    "OpenSearch returned inconsistent E2E workload metadata in one result page.");
            }

            workload = candidate;
        }
    }

    return new SearchPage(scrollId, eventKeys, workload);
}

static void PrintSample(string title, IReadOnlyCollection<string> eventKeys)
{
    if (eventKeys.Count == 0)
    {
        return;
    }

    Console.Error.WriteLine($"{title} sample: {string.Join(", ", eventKeys.Take(10))}");
}

sealed class DlqCollector : IDisposable
{
    private readonly IConsumer<Ignore, string> _consumer;
    private readonly string _runId;
    private readonly CancellationTokenSource _stop = new();
    private readonly ConcurrentDictionary<string, byte> _eventKeys = new(StringComparer.Ordinal);
    private readonly CancellationToken _timeoutToken;
    private Task? _consumeTask;
    private int _duplicateEventKeys;
    private E2eWorkload? _workload;

    public DlqCollector(VerifierOptions options, CancellationToken timeoutToken)
    {
        _runId = options.RunId;
        _timeoutToken = timeoutToken;

        _consumer = new ConsumerBuilder<Ignore, string>(new ConsumerConfig
        {
            BootstrapServers = options.KafkaBootstrapServers,
            GroupId = $"traceflow-e2e-verifier-{Guid.NewGuid():N}",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            EnablePartitionEof = true
        }).Build();

        _consumer.Subscribe(options.KafkaDlqTopic);
    }

    public IReadOnlyCollection<string> EventKeys => _eventKeys.Keys.ToArray();

    public int DuplicateEventKeys => Volatile.Read(ref _duplicateEventKeys);

    public E2eWorkload? Workload => Volatile.Read(ref _workload);

    public void Start()
    {
        _consumeTask = Task.Run(ConsumeLoopAsync);
    }

    public async Task StopAsync()
    {
        _stop.Cancel();

        if (_consumeTask is not null)
        {
            await _consumeTask;
        }
    }

    private async Task ConsumeLoopAsync()
    {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(
            _stop.Token,
            _timeoutToken);

        try
        {
            while (!linked.IsCancellationRequested)
            {
                ConsumeResult<Ignore, string>? result = null;

                try
                {
                    result = _consumer.Consume(TimeSpan.FromMilliseconds(250));
                }
                catch (ConsumeException exception)
                {
                    Console.Error.WriteLine($"DLQ consume error: {exception.Error.Reason}");
                    await Task.Delay(TimeSpan.FromSeconds(1), linked.Token);
                    continue;
                }

                if (result?.Message?.Value is not { Length: > 0 } value)
                {
                    continue;
                }

                TryRecordEventKey(value);
            }
        }
        catch (OperationCanceledException) when (linked.IsCancellationRequested)
        {
        }
    }

    private void TryRecordEventKey(string value)
    {
        try
        {
            using var document = JsonDocument.Parse(value);
            var root = document.RootElement;

            if (!root.TryGetProperty("event", out var logEvent) ||
                logEvent.ValueKind != JsonValueKind.Object ||
                !logEvent.TryGetProperty("metadata", out var metadata) ||
                !metadata.TryGetProperty("e2eRunId", out var runId) ||
                !string.Equals(runId.GetString(), _runId, StringComparison.Ordinal) ||
                !metadata.TryGetProperty("e2eEventKey", out var eventKey) ||
                !metadata.TryGetProperty("e2eExpectedVus", out var virtualUsers) ||
                !metadata.TryGetProperty("e2eExpectedIterationsPerVu", out var iterationsPerVu) ||
                !E2eValueParser.TryGetPositiveInteger(virtualUsers, out var vus) ||
                !E2eValueParser.TryGetPositiveInteger(iterationsPerVu, out var iterations))
            {
                return;
            }

            var workload = new E2eWorkload(vus, iterations);
            var existingWorkload = Interlocked.CompareExchange(
                ref _workload,
                workload,
                null);

            if (existingWorkload is not null && existingWorkload != workload)
            {
                throw new InvalidOperationException(
                    "DLQ returned inconsistent E2E workload metadata for the same run.");
            }

            var key = eventKey.GetString();

            if (!string.IsNullOrWhiteSpace(key) && !_eventKeys.TryAdd(key, 0))
            {
                Interlocked.Increment(ref _duplicateEventKeys);
            }
        }
        catch (JsonException)
        {
        }
    }

    public void Dispose()
    {
        _stop.Cancel();
        _consumer.Close();
        _consumer.Dispose();
        _stop.Dispose();
    }
}

sealed record SearchPage(
    string? ScrollId,
    IReadOnlyList<string> EventKeys,
    E2eWorkload? Workload);

sealed record OpenSearchEvents(
    IReadOnlySet<string> EventKeys,
    int DuplicateEventKeys,
    E2eWorkload? Workload)
{
    public static OpenSearchEvents Empty { get; } =
        new(new HashSet<string>(StringComparer.Ordinal), 0, null);
}

sealed record E2eWorkload(int VirtualUsers, int IterationsPerVu)
{
    public int ExpectedEventCount => checked(VirtualUsers * IterationsPerVu);
}

static class E2eValueParser
{
    public static bool TryGetPositiveInteger(JsonElement element, out int value)
    {
        if (element.TryGetInt32(out value))
        {
            return value > 0;
        }

        if (element.TryGetDouble(out var floatingPointValue) &&
            floatingPointValue > 0 &&
            floatingPointValue <= int.MaxValue &&
            Math.Floor(floatingPointValue) == floatingPointValue)
        {
            value = (int)floatingPointValue;
            return true;
        }

        value = default;
        return false;
    }
}

sealed record VerifierOptions(
    string RunId,
    int TimeoutSeconds,
    string KafkaBootstrapServers,
    string KafkaDlqTopic,
    string OpenSearchUrl,
    string OpenSearchUsername,
    string OpenSearchPassword,
    string OpenSearchIndex,
    bool SkipTlsVerify)
{
    public static VerifierOptions FromEnvironment()
    {
        return new VerifierOptions(
            Required("E2E_RUN_ID"),
            PositiveInteger("E2E_TIMEOUT_SECONDS"),
            Required("Kafka__BootstrapServers"),
            Environment.GetEnvironmentVariable("Kafka__DlqTopic") ?? "traceflow.logs.dlq",
            Required("OpenSearch__Url"),
            Required("OpenSearch__Username"),
            Required("OpenSearch__Password"),
            Required("OpenSearch__Index"),
            bool.TryParse(Environment.GetEnvironmentVariable("OpenSearch__SkipTlsVerify"), out var skipTlsVerify) &&
            skipTlsVerify);
    }

    private static string Required(string name)
    {
        return Environment.GetEnvironmentVariable(name) is { Length: > 0 } value
            ? value
            : throw new InvalidOperationException($"Environment variable {name} is required.");
    }

    private static int PositiveInteger(string name)
    {
        return int.TryParse(Environment.GetEnvironmentVariable(name), out var value) && value > 0
            ? value
            : throw new InvalidOperationException(
                $"Environment variable {name} must be a positive integer.");
    }
}
