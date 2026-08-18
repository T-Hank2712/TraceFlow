using Microsoft.EntityFrameworkCore;
using TraceFlow.api.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

var app = builder.Build();

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
