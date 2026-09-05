using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Infrastructure.Persistence;
using TraceFlow.Api.Application.Common.Behaviors;
using MediatR;
using TraceFlow.Api.Middleware;
using TraceFlow.Api.Application.Common.Security;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

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

var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("JWT secret is not configured.");

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Description = "Enter JWT access token only. Swagger UI will add the Bearer prefix."
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

                operation.Security.Add(new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                });
            }
        }

        return Task.CompletedTask;
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(postgresConnectionString));

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(typeof(Program).Assembly);
});

builder.Services.AddControllers();
builder.Services.AddScoped<PasswordHasher>();
builder.Services.AddScoped<JwtTokenGenerator>();
builder.Services.AddScoped<RefreshTokenGenerator>();

builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(ValidationBehavior<,>));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(
    options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSecret))
        };
    }
);

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "TraceFlow API v1");
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/health", () =>
{
    return Results.Ok("TraceFlow Control API is running.");
})
.WithName("HealthCheck");

app.MapGet("/health/database", async (AppDbContext dbContext) =>
{
    try
    {
        var connection = dbContext.Database.GetDbConnection();

        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "select current_database()";

        var databaseName = await command.ExecuteScalarAsync();

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
            message = "Failed to connect to the database.",
            error = ex.Message
        });
    }
});

app.MapGet("/health/database/config", (IConfiguration configuration, IWebHostEnvironment env) =>
{
    var connectionString = configuration.GetConnectionString("Postgres");

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        return Results.Ok(new
        {
            env.ContentRootPath,
            connectionString = "empty"
        });
    }

    var builder = new Npgsql.NpgsqlConnectionStringBuilder(connectionString);

    return Results.Ok(new
    {
        env.ContentRootPath,
        builder.Host,
        builder.Port,
        builder.Database,
        builder.Username
    });
});

app.Run();
