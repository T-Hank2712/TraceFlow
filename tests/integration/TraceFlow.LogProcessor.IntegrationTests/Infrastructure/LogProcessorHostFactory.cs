using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenSearch.Client;
using TraceFlow.LogProcessor.Configurations;
using TraceFlow.LogProcessor.Processing;
using TraceFlow.LogProcessor.Services.Batching;
using TraceFlow.LogProcessor.Services.Kafka;
using TraceFlow.LogProcessor.Services.Offsets;
using TraceFlow.LogProcessor.Services.OpenSearch;
using TraceFlow.LogProcessor.Workers;

namespace TraceFlow.LogProcessor.IntegrationTests.Infrastructure;

public static class LogProcessorHostFactory
{
    public static IHost Create(
        KafkaFixture kafka,
        OpenSearchFixture openSearch,
        string groupId,
        int batchSize = 1,
        int flushIntervalMs = 200,
        Action<IServiceCollection>? configureServices = null)
    {
        var config = new Dictionary<string, string?>
        {
            ["Kafka:BootstrapServers"] = kafka.BootstrapServers,
            ["Kafka:GroupId"] = groupId,
            ["Kafka:Topic"] = "traceflow.logs",
            ["Kafka:DlqTopic"] = "traceflow.logs.dlq",
            ["Kafka:PollTimeoutMs"] = "100",

            ["Processor:MaxRetries"] = "1",
            ["Processor:RetryBackoffMs"] = "50",
            ["Processor:BatchSize"] = batchSize.ToString(),
            ["Processor:FlushIntervalMs"] = flushIntervalMs.ToString(),

            ["OpenSearch:Url"] = openSearch.Url,
            ["OpenSearch:Username"] = openSearch.Username,
            ["OpenSearch:Password"] = openSearch.Password,
            ["OpenSearch:Index"] = openSearch.Index,
            ["OpenSearch:SkipTlsVerify"] = "true"
        };

        var builder = Host.CreateApplicationBuilder();

        builder.Configuration.AddInMemoryCollection(config);

        var openSearchSettings = new ConnectionSettings(new Uri(openSearch.Url))
            .BasicAuthentication(openSearch.Username, openSearch.Password)
            .DefaultIndex(openSearch.Index)
            .ServerCertificateValidationCallback((_, _, _, _) => true);

        builder.Services
            .AddOptions<KafkaOptions>()
            .Bind(builder.Configuration.GetSection("Kafka"))
            .ValidateOnStart();

        builder.Services
            .AddOptions<ProcessorOptions>()
            .Bind(builder.Configuration.GetSection("Processor"))
            .ValidateOnStart();

        builder.Services
            .AddOptions<OpenSearchOptions>()
            .Bind(builder.Configuration.GetSection("OpenSearch"))
            .ValidateOnStart();

        builder.Services.AddSingleton<IKafkaConsumer, KafkaConsumer>();
        builder.Services.AddSingleton<ILogEventNormalizer, LogEventNormalizer>();
        builder.Services.AddSingleton<IBatchProcessor, BatchProcessor>();
        builder.Services.AddSingleton<ILogIndexer, LogIndexer>();
        builder.Services.AddSingleton<IOpenSearchClient>(new OpenSearchClient(openSearchSettings));
        builder.Services.AddSingleton<IDlqProducer, DlqProducer>();
        builder.Services.AddSingleton<IOffsetCoordinator, OffsetCoordinator>();
        builder.Services.AddSingleton<IOffsetCommitSignal, OffsetCommitSignal>();

        builder.Services.AddHostedService<LogConsumerWorker>();
        builder.Services.AddHostedService<BatchFlushWorker>();

        configureServices?.Invoke(builder.Services);

        return builder.Build();
    }
}