var builder = WebApplication.CreateBuilder(args);

var envPath = Path.Combine(
    builder.Environment.ContentRootPath,
    ".env");

if (File.Exists(envPath))
{
    Env.Load(envPath);
    builder.Configuration.AddEnvironmentVariables();
}

var postgresConnectionString =
    builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException(
        "Postgres connection string is not configured.");

var jwtSecret =
    builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException(
        "JWT secret is not configured.");

var openSearchUrl =
    builder.Configuration["OpenSearch:Url"]
    ?? throw new InvalidOperationException(
        "OpenSearch URL is not configured.");

var openSearchUsername =
    builder.Configuration["OpenSearch:Username"]
    ?? throw new InvalidOperationException(
        "OpenSearch username is not configured.");

var openSearchPassword =
    builder.Configuration["OpenSearch:Password"]
    ?? throw new InvalidOperationException(
        "OpenSearch password is not configured.");

var openSearchIndex =
    builder.Configuration["OpenSearch:Index"]
    ?? throw new InvalidOperationException(
        "OpenSearch index is not configured.");

var openSearchSkipTlsVerify =
    builder.Configuration.GetValue<bool>(
        "OpenSearch:SkipTlsVerify");

// Database
builder.Services.AddDbContext<AppDbContext>(
    options =>
        options.UseNpgsql(postgresConnectionString));

// Validation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// MediatR
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(
        typeof(Program).Assembly);
});

// MVC / Controllers
builder.Services.AddControllers();

// API Versioning + OpenAPI
builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = false;
        options.ReportApiVersions = true;
        options.ApiVersionReader =
            new UrlSegmentApiVersionReader();
    })
    .AddMvc()
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    })
    .AddOpenApi(options =>
    {
        options.Document.AddDocumentTransformer(
            (document, context, cancellationToken) =>
            {
                document.Servers =
                [
                    new OpenApiServer
                    {
                        Url = "/control"
                    }
                ];

                document.Components ??=
                    new OpenApiComponents();

                document.Components.SecuritySchemes ??=
                    new Dictionary<
                        string,
                        IOpenApiSecurityScheme>();

                document.Components.SecuritySchemes["Bearer"] =
                    new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Description =
                            "Enter JWT access token only. " +
                            "Swagger UI will add the Bearer prefix."
                    };

                foreach (var path in document.Paths.Values)
                {
                    if (path.Operations is null)
                    {
                        continue;
                    }

                    foreach (var operation in path.Operations.Values)
                    {
                        operation.Security ??= [];

                        operation.Security.Add(
                            new OpenApiSecurityRequirement
                            {
                                [
                                    new OpenApiSecuritySchemeReference(
                                        "Bearer",
                                        document)
                                ] = []
                            });
                    }
                }

                return Task.CompletedTask;
            });
    });

// Application services
builder.Services.AddScoped<PasswordHasher>();
builder.Services.AddScoped<JwtTokenGenerator>();
builder.Services.AddScoped<RefreshTokenGenerator>();
builder.Services.AddScoped<WorkspaceAccessService>();
builder.Services.AddScoped<ProjectAccessService>();
builder.Services.AddScoped<UserLookupService>();
builder.Services.AddScoped<ApiKeyGenerator>();
builder.Services.AddScoped<ApiKeyHasher>();
builder.Services.AddScoped<ApiKeyExpirationPolicyResolver>();
builder.Services.AddScoped<ApiKeyParser>();

builder.Services.AddSingleton(TimeProvider.System);

// MediatR validation behavior
builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(ValidationBehavior<,>));

// Authentication
builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer =
                    builder.Configuration["Jwt:Issuer"],

                ValidAudience =
                    builder.Configuration["Jwt:Audience"],

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSecret))
            };
    });

// Authorization
builder.Services.AddAuthorization();

// OpenSearch
builder.Services.AddHttpClient<
    ILogSearchReader,
    OpenSearchLogSearchReader>(client =>
{
    client.BaseAddress = new Uri(openSearchUrl);

    var credentials =
        Convert.ToBase64String(
            Encoding.UTF8.GetBytes(
                $"{openSearchUsername}:{openSearchPassword}"));

    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue(
            "Basic",
            credentials);
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    return new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback =
            openSearchSkipTlsVerify
                ? HttpClientHandler
                    .DangerousAcceptAnyServerCertificateValidator
                : null
    };
});

var app = builder.Build();

// Exception handling
app.UseMiddleware<ExceptionHandlingMiddleware>();

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
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Versioned Controllers
app.MapControllers();

// Health
app.MapGet("/health", () =>
{
    return Results.Ok(
        "TraceFlow Control API is running.");
})
.WithName("HealthCheck");

// Database health
app.MapGet(
    "/health/database",
    async (AppDbContext dbContext) =>
    {
        try
        {
            var connection =
                dbContext.Database.GetDbConnection();

            await connection.OpenAsync();

            await using var command =
                connection.CreateCommand();

            command.CommandText =
                "select current_database()";

            var databaseName =
                await command.ExecuteScalarAsync();

            return Results.Ok(new
            {
                message = "Database is reachable.",
                database = databaseName
            });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new
            {
                message =
                    "Failed to connect to the database.",
                error = ex.Message
            });
        }
    })
.WithName("DatabaseHealthCheck");

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

// OpenSearch configuration health
app.MapGet(
    "/health/opensearch/config",
    () =>
    {
        return Results.Ok(new
        {
            url = openSearchUrl,
            username = openSearchUsername,
            index = openSearchIndex,
            skipTlsVerify = openSearchSkipTlsVerify
        });
    })
.WithName("OpenSearchConfigurationHealthCheck");

app.Run();
