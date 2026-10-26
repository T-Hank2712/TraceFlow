using TraceFlow.LogProcessor.Configurations;
using DotNetEnv;
using TraceFlow.LogProcessor.Services.Kafka;
using TraceFlow.LogProcessor.Services.Batching;
using TraceFlow.LogProcessor.Workers;
using TraceFlow.LogProcessor.Processing;
using TraceFlow.LogProcessor.Services.OpenSearch;
using OpenSearch.Client;
using TraceFlow.LogProcessor.Services.Offsets;
using TraceFlow.LogProcessor.Services.Queue;

Env.Load();
var builder = Host.CreateApplicationBuilder(args);

var openSearchOptions = builder.Configuration
    .GetSection("OpenSearch")
    .Get<OpenSearchOptions>()
    ?? throw new InvalidOperationException(
        "OpenSearch options are not configured.");

var settings = new ConnectionSettings(new Uri(openSearchOptions.Url))
    .BasicAuthentication(
        openSearchOptions.Username,
        openSearchOptions.Password)
    .DefaultIndex(openSearchOptions.Index);

if (openSearchOptions.SkipTlsVerify)
{
    settings = settings.ServerCertificateValidationCallback(
        (sender, certificate, chain, errors) => true);
}

builder.Services
    .AddOptions<KafkaOptions>()
    .Bind(builder.Configuration.GetSection("Kafka"))
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.BootstrapServers),
        "Kafka__BootstrapServers is required.")
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.GroupId),
        "Kafka__GroupId is required.")
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.Topic),
        "Kafka__Topic is required.")
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.DlqTopic),
        "Kafka__DlqTopic is required.")
    .Validate(
        options => options.PollTimeoutMs > 0,
        "Kafka__PollTimeoutMs must be greater than 0.")
    .Validate(
        options => options.ConsumeFailureTimeoutSeconds > 0,
        "Kafka__ConsumeFailureTimeoutSeconds must be greater than 0.")
    .Validate(
        options => options.ConsumeFailureBackoffMs > 0,
        "Kafka__ConsumeFailureBackoffMs must be greater than 0.")
    .Validate(
        options => options.ProducerFlushTimeoutSeconds > 0,
        "Kafka__ProducerFlushTimeoutSeconds must be greater than 0.")
    .Validate(
        options => options.DlqPublishConcurrency > 0,
        "Kafka__DlqPublishConcurrency must be greater than 0.")
    .Validate(
        options => options.QueuedMinMessages > 0,
        "Kafka__QueuedMinMessages must be greater than 0.")
    .Validate(
        options => options.QueuedMaxMessagesKbytes > 0,
        "Kafka__QueuedMaxMessagesKbytes must be greater than 0.")
    .Validate(
        options => options.FetchMinBytes > 0,
        "Kafka__FetchMinBytes must be greater than 0.")
    .Validate(
        options => options.FetchWaitMaxMs >= 0,
        "Kafka__FetchWaitMaxMs must be greater than or equal to 0.")
    .Validate(
        options => options.MaxPartitionFetchBytes > 0,
        "Kafka__MaxPartitionFetchBytes must be greater than 0.")
    .Validate(
        options => options.NoPartitionAssignmentTimeoutSeconds > 0,
        "Kafka__NoPartitionAssignmentTimeoutSeconds must be greater than 0.")
    .Validate(
        options => options.MaxSubscriptionRecoveryAttempts > 0,
        "Kafka__MaxSubscriptionRecoveryAttempts must be greater than 0.")
    .ValidateOnStart();

builder.Services
    .AddOptions<ProcessorOptions>()
    .Bind(builder.Configuration.GetSection("Processor"))
    .Validate(
        options => options.MaxRetries >= 0,
        "Processor__MaxRetries must be greater than or equal to 0.")
    .Validate(
        options => options.RetryBackoffMs >= 0,
        "Processor__RetryBackoffMs must be greater than or equal to 0.")
    .Validate(
        options => options.BatchSize > 0,
        "Processor__BatchSize must be greater than 0.")
    .Validate(
        options => options.FlushIntervalMs > 0,
        "Processor__FlushIntervalMs must be greater than 0.")
    .Validate(
        options => options.QueueCapacity > 0,
        "Processor__QueueCapacity must be greater than 0.")
    .Validate(
        options => options.ShutdownFlushTimeoutSeconds > 0,
        "Processor__ShutdownFlushTimeoutSeconds must be greater than 0.")
    .ValidateOnStart();

builder.Services
    .AddOptions<OpenSearchOptions>()
    .Bind(builder.Configuration.GetSection("OpenSearch"))
    .Validate(
        options => Uri.TryCreate(options.Url, UriKind.Absolute, out _),
        "OpenSearch__Url must be a valid absolute URL.")
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.Username),
        "OpenSearch__Username is required.")
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.Password),
        "OpenSearch__Password is required.")
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.Index),
        "OpenSearch__Index is required.")
    .ValidateOnStart();

builder.Services.AddSingleton<IKafkaConsumer, KafkaConsumer>();
builder.Services.AddSingleton<ILogEventNormalizer, LogEventNormalizer>();
builder.Services.AddSingleton<ILogIndexer, LogIndexer>();
builder.Services.AddSingleton<IOpenSearchClient>(new OpenSearchClient(settings));
builder.Services.AddSingleton<IDlqProducer, DlqProducer>();
builder.Services.AddSingleton<IOffsetCoordinator, OffsetCoordinator>();
builder.Services.AddSingleton<IOffsetCommitSignal, OffsetCommitSignal>();

builder.Services.AddTransient<IBatchProcessor, BatchProcessor>();

builder.Services.AddSingleton<IBatchProcessorFactory, BatchProcessorFactory>();

builder.Services.AddHostedService<LogConsumerWorker>();

builder.Services.AddSingleton<IPartitionLaneManager, PartitionLaneManager>();


var host = builder.Build();
host.Run();
