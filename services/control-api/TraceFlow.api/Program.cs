using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Infrastructure.Persistence;
using TraceFlow.Api.Application.Common.Behaviors;
using MediatR;
using TraceFlow.Api.Middleware;
using TraceFlow.Api.Application.Common.Security;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);
var envPath = Path.Combine(
    builder.Environment.ContentRootPath,
    ".env");

if (File.Exists(envPath))
{
    Env.Load(envPath);
    builder.Configuration.AddEnvironmentVariables();
}

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var postgresConnectionString =
    builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException(
        "Postgres connection string is not configured.");

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
