DotNetEnv.Env.NoClobber().Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplicationOptions(builder.Configuration)
    .AddApiDocumentation()
    .AddIngestionServices()
    .AddControlApiClient(builder.Configuration)
    .AddKafkaProducer()
    .AddRedisInfrastructure(builder.Configuration)
    .AddIngestionHealthChecks(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().WithDocumentPerVersion();
}

app.UseHttpsRedirection();

app.UseIngestionPipeline();

app.MapHealthEndpoints();
app.MapLogIngestionEndpoints();
app.MapBatchLogEndpoints();

app.Run();
