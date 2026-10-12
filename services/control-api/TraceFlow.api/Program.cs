var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentFile(builder.Environment);

builder.Services.AddTypedOptions(builder.Configuration);

builder.Services.AddPersistence(builder.Configuration);

builder.Services.AddApplicationServices();

builder.Services.AddJwtSecurity(builder.Configuration);

builder.Services.AddOpenSearch(
    builder.Configuration,
    builder.Environment);

builder.Services.AddApiDocumentation();

builder.Services.AddApiRateLimiting(builder.Configuration);

builder.Services.AddForwardedClientHeaders(builder.Configuration, builder.Environment);

var app = builder.Build();

// OpenAPI / Swagger
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi()
        .WithDocumentPerVersion();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/control/openapi/v1.json",
            "TraceFlow Control API v1");
    });

    // OpenSearch configuration health
    app.MapGet(
        "/health/opensearch/config",
        (IOptions<OpenSearchOptions> options) =>
        {
            var openSearchOptions = options.Value;

            return Results.Ok(new
            {
                url = openSearchOptions.Url,
                username = openSearchOptions.Username,
                index = openSearchOptions.Index,
                skipTlsVerify = openSearchOptions.SkipTlsVerify
            });
        })
    .WithName("OpenSearchConfigurationHealthCheck");

    // Database configuration health
    app.MapGet(
        "/health/database/config",
        (
            IConfiguration configuration,
            IWebHostEnvironment env) =>
        {
            var connectionString =
                configuration.GetConnectionString(
                    "Postgres");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return Results.Ok(new
                {
                    env.ContentRootPath,
                    connectionString = "empty"
                });
            }

            var connectionStringBuilder =
                new Npgsql.NpgsqlConnectionStringBuilder(
                    connectionString);

            return Results.Ok(new
            {
                env.ContentRootPath,
                connectionStringBuilder.Host,
                connectionStringBuilder.Port,
                connectionStringBuilder.Database,
                connectionStringBuilder.Username
            });
        })
    .WithName("DatabaseConfigurationHealthCheck");
}

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseForwardedHeaders();

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();

// Versioned Controllers
app.MapControllers();

app.MapHealthChecks("/health")
.AllowAnonymous();

app.Run();
