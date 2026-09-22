using TraceFlow.LogProcessor;
using TraceFlow.LogProcessor.Configurations;
using DotNetEnv;
using TraceFlow.LogProcessor.Services.Kafka;
using TraceFlow.LogProcessor.Services.Batching;
using TraceFlow.LogProcessor.Workers;
using TraceFlow.LogProcessor.Processing;

Env.Load();
var builder = Host.CreateApplicationBuilder(args);

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

builder.Services.AddHostedService<LogConsumerWorker>();
builder.Services.AddHostedService<BatchFlushWorker>();

var host = builder.Build();
host.Run();
