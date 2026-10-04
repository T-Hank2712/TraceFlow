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

builder.Services
    .AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .Validate(options => !string.IsNullOrWhiteSpace(options.Issuer), "JWT issuer is required.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.Audience), "JWT audience is required.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.Secret), "JWT secret is required.")
    .Validate(options => options.AccessTokenExpirationMinutes > 0, "JWT access token expiration must be greater than 0.")
    .Validate(options => options.RefreshTokenExpirationDays > 0, "JWT refresh token expiration must be greater than 0.")
    .ValidateOnStart();

builder.Services
    .AddOptions<OpenSearchOptions>()
    .Bind(builder.Configuration.GetSection(OpenSearchOptions.SectionName))
    .Validate(options => !string.IsNullOrWhiteSpace(options.Url), "OpenSearch URL is required.")
    .Validate(options => Uri.TryCreate(options.Url, UriKind.Absolute, out _), "OpenSearch URL must be absolute.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.Username), "OpenSearch username is required.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.Password), "OpenSearch password is required.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.Index), "OpenSearch index is required.")
    .ValidateOnStart();

builder.Services
    .AddOptions<ApiKeySecurityOptions>()
    .Bind(builder.Configuration.GetSection(ApiKeySecurityOptions.SectionName))
    .Validate(options => !string.IsNullOrWhiteSpace(options.Pepper), "API key pepper is required.")
    .ValidateOnStart();

builder.Services
    .AddOptions<InternalServiceOptions>()
    .Bind(builder.Configuration.GetSection(InternalServiceOptions.SectionName))
    .Validate(options => !string.IsNullOrWhiteSpace(options.Secret), "Internal service secret is required.")
    .ValidateOnStart();

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
var jwtOptions =
    builder.Configuration
        .GetSection(JwtOptions.SectionName)
        .Get<JwtOptions>()
    ?? throw new InvalidOperationException("JWT options are not configured.");

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

                ValidIssuer = jwtOptions.Issuer,
                ValidAudience = jwtOptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtOptions.Secret))
            };
    });

// Authorization
builder.Services.AddAuthorization();

// OpenSearch
var openSearchOptions =
    builder.Configuration
        .GetSection(OpenSearchOptions.SectionName)
        .Get<OpenSearchOptions>()
    ?? throw new InvalidOperationException("OpenSearch options are not configured.");

builder.Services.AddHttpClient<
    ILogSearchReader,
    OpenSearchLogSearchReader>(client =>
{
    client.BaseAddress = new Uri(openSearchOptions.Url);

    var credentials =
        Convert.ToBase64String(
            Encoding.UTF8.GetBytes(
                $"{openSearchOptions.Username}:{openSearchOptions.Password}"));

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
            openSearchOptions.SkipTlsVerify
                ? HttpClientHandler
                    .DangerousAcceptAnyServerCertificateValidator
                : null
    };
});

var app = builder.Build();

// Exception handling
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

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
            url = openSearchOptions.Url,
            username = openSearchOptions.Username,
            index = openSearchOptions.Index,
            skipTlsVerify = openSearchOptions.SkipTlsVerify
        });
    })
.WithName("OpenSearchConfigurationHealthCheck");

app.Run();
