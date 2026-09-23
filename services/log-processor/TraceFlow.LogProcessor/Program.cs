using TraceFlow.LogProcessor.Configurations;
using DotNetEnv;
using TraceFlow.LogProcessor.Services.Kafka;
using TraceFlow.LogProcessor.Services.Batching;
using TraceFlow.LogProcessor.Workers;
using TraceFlow.LogProcessor.Processing;
using TraceFlow.LogProcessor.Services.OpenSearch;
using OpenSearch.Client;

Env.Load();
var builder = Host.CreateApplicationBuilder(args);

var openSearchUrl = builder.Configuration["OpenSearch:Url"]
    ?? throw new InvalidOperationException(
        "OpenSearch URL is not configured.");

var username = builder.Configuration["OpenSearch:Username"]
    ?? throw new InvalidOperationException(
        "OpenSearch username is not configured.");

var password = builder.Configuration["OpenSearch:Password"]
    ?? throw new InvalidOperationException(
        "OpenSearch password is not configured.");

var index = builder.Configuration["OpenSearch:Index"]
    ?? throw new InvalidOperationException(
        "OpenSearch index is not configured.");

var settings = new ConnectionSettings(new Uri(openSearchUrl))
    .BasicAuthentication(username, password)
    .DefaultIndex(index)
    .ServerCertificateValidationCallback(
        (sender, certificate, chain, errors) => true);

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
builder.Services.AddSingleton<IOpenSearchClient>(new OpenSearchClient(settings));
builder.Services.AddSingleton<IDlqProducer, DlqProducer>();

builder.Services.AddHostedService<LogConsumerWorker>();
builder.Services.AddHostedService<BatchFlushWorker>();

var host = builder.Build();
host.Run();
