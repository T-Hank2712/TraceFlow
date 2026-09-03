using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Infrastructure.Persistence;
using TraceFlow.Api.Application.Common.Behaviors;
using MediatR;
using TraceFlow.Api.Middleware;
using TraceFlow.Api.Application.Common.Security;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(typeof(Program).Assembly);
});

builder.Services.AddControllers();
builder.Services.AddScoped<PasswordHasher>();

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

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/health", () =>
{
    return Results.Ok("TraceFlow Control API is running.");
})
.WithName("HealthCheck");

app.MapGet("/health/database", async (AppDbContext dbContext) =>
{
    try
    {
        await dbContext.Database.CanConnectAsync();
        return Results.Ok("Database is reachable.");
    }
    catch (Exception)
    {
        return Results.BadRequest("Failed to connect to the database.");
    }
})
.WithName("DatabaseHealthCheck");
app.Run();
