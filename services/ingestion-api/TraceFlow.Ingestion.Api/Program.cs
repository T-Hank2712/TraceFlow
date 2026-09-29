using TraceFlow.Ingestion.Api.Configuration;
using TraceFlow.Ingestion.Api.Endpoints;
using TraceFlow.Ingestion.Api.Clients;
using TraceFlow.Ingestion.Api.Services.Ingestion;
using TraceFlow.Ingestion.Api.Kafka;
using TraceFlow.Ingestion.Api.Security;
using FluentValidation;
using TraceFlow.Ingestion.Api.Contracts.BatchLog;
using TraceFlow.Ingestion.Api.Contracts.Log;
using StackExchange.Redis;
using TraceFlow.Ingestion.Api.Services.Redis;
using TraceFlow.Ingestion.Api.Services.RateLimiting;
using TraceFlow.Ingestion.Api.Extensions;
using TraceFlow.Ingestion.Api.Middleware;
using Asp.Versioning;

DotNetEnv.Env.NoClobber().Load();
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services
    .AddOptions<ControlApiOptions>()
    .Bind(builder.Configuration.GetSection("ControlApi"))
    .Validate(options => !string.IsNullOrWhiteSpace(options.BaseUrl), "ControlApi__BaseUrl is required.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.ValidateApiPath), "ControlApi__ValidateApiPath is required.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.InternalServiceSecret), "ControlApi__InternalServiceSecret is required.")
    .Validate(options => options.TimeoutSeconds > 0)
    .ValidateOnStart();

builder.Services
    .AddOptions<KafkaOptions>()
    .Bind(builder.Configuration.GetSection("Kafka"))
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.BootstrapServers),
        "Kafka bootstrap servers are required.")
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.Topic),
        "Kafka topic is required.")
    .Validate(
        options => options.MessageSendMaxRetries >= 0,
        "MessageSendMaxRetries must be greater than or equal to 0.")
    .Validate(
        options => options.RetryBackoffMs >= 0,
        "RetryBackoffMs must be greater than or equal to 0.")
    .Validate(
        options => options.DeliveryTimeoutMs > 0,
        "DeliveryTimeoutMs must be greater than 0.")
    .Validate(
        options => options.MessageTimeoutMs > 0,
        "MessageTimeoutMs must be greater than 0.")
    .Validate(
        options => options.QueueBufferingMaxMessages > 0,
        "QueueBufferingMaxMessages must be greater than 0.")
    .Validate(
        options => options.QueueBufferingMaxKbytes > 0,
        "QueueBufferingMaxKbytes must be greater than 0.")
    .Validate(
        options => options.LingerMs >= 0,
        "LingerMs must be greater than or equal to 0.")
    .ValidateOnStart();

builder.Services
    .AddOptions<IngestionOptions>()
    .Bind(builder.Configuration.GetSection("Ingestion"))
    .Validate(options => options.MaxBatchSize > 0, "Ingestion__MaxBatchSize must be greater than 0.")
    .Validate(options => options.MaxRequestBodyBytes > 0, "Ingestion__MaxRequestBodyBytes must be greater than 0.")
    .Validate(options => options.MaxMessageLength > 0, "Ingestion__MaxMessageLength must be greater than 0.")
    .Validate(options => options.MaxServiceLength > 0, "Ingestion__MaxServiceLength must be greater than 0.")
    .ValidateOnStart();

builder.Services
    .AddOptions<RedisOptions>()
    .Bind(builder.Configuration.GetSection(RedisOptions.SectionName))
    .ValidateOnStart();

builder.Services
    .AddOptions<RateLimitOptions>()
    .Bind(builder.Configuration.GetSection(RateLimitOptions.SectionName))
    .ValidateOnStart();

var redisConnectionString =
    builder.Configuration.GetSection(RedisOptions.SectionName)["ConnectionString"]
    ?? throw new InvalidOperationException(
        "Redis connection string is not configured.");

builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = false;
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    })
    .AddMvc()
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

builder.Services.AddSingleton<ApiKeyHeaderParser>();
builder.Services.AddSingleton<EnrichedLogEventFactory>();
builder.Services.AddScoped<Authenticator>();
builder.Services.AddScoped<IValidator<BatchLogRequest>, BatchLogRequestValidator>();
builder.Services.AddScoped<IValidator<IngestLogRequest>, IngestLogRequestValidator>();

builder.Services.AddHttpClient<IApiKeyValidator, ControlApiClient>();

builder.Services.AddSingleton<ILogEventPublisher, KafkaLogProducer>();

builder.Services.AddScoped<IIngestLogService, IngestLogService>();
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));
builder.Services.AddSingleton<IRedisCache, RedisCache>();
builder.Services.AddSingleton<IRateLimiter, RedisRateLimiter>();
builder.Services.AddSingleton<TenantContextCacheKey>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapHealthEndpoints();
app.MapLogIngestionEndpoints();
app.MapBatchLogEndpoints();
app.UseRequestBodySizeLimit();
app.UseMiddleware<AuthenticationMiddleware>();
app.UseMiddleware<RateLimitingMiddleware>();

app.Run();
