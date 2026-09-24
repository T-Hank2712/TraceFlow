using TraceFlow.Ingestion.Api.Configuration;
using TraceFlow.Ingestion.Api.Endpoints;
using TraceFlow.Ingestion.Api.Clients;
using TraceFlow.Ingestion.Api.Ingestion;
using TraceFlow.Ingestion.Api.Kafka;
using TraceFlow.Ingestion.Api.Security;
using FluentValidation;
using TraceFlow.Ingestion.Api.Contracts.BatchLog;
using TraceFlow.Ingestion.Api.Contracts.Log;
using StackExchange.Redis;
using TraceFlow.Ingestion.Api.Configurations;

DotNetEnv.Env.Load();
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
    .Validate(options => !string.IsNullOrWhiteSpace(options.BootstrapServers), "Kafka__BootstrapServers is required.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.Topic), "Kafka__Topic is required.")
    .Validate(options => options.DeliveryTimeoutSeconds > 0, "Kafka__DeliveryTimeoutSeconds must be greater than 0.")
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

var redisConnectionString =
    builder.Configuration.GetSection(RedisOptions.SectionName)["ConnectionString"]
    ?? throw new InvalidOperationException(
        "Redis connection string is not configured.");

builder.Services.AddSingleton<ApiKeyHeaderParser>();
builder.Services.AddSingleton<EnrichedLogEventFactory>();
builder.Services.AddScoped<Authenticator>();
builder.Services.AddScoped<IValidator<BatchLogRequest>, BatchLogRequestValidator>();
builder.Services.AddScoped<IValidator<IngestLogRequest>, IngestLogRequestValidator>();

builder.Services.AddHttpClient<IApiKeyValidator, ControlApiClient>();

builder.Services.AddSingleton<ILogEventPublisher, KafkaLogProducer>();

builder.Services.AddScoped<IIngestLogService, IngestLogService>();
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));

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

app.Run();
