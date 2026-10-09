DotNetEnv.Env.NoClobber().Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplicationOptions(builder.Configuration)
    .AddApiDocumentation()
    .AddIngestionServices()
    .AddControlApiClient(builder.Configuration)
    .AddKafkaProducer()
    .AddRedisInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().WithDocumentPerVersion();
}

app.UseHttpsRedirection();

app.MapHealthEndpoints();
app.MapLogIngestionEndpoints();
app.MapBatchLogEndpoints();

app.UseIngestionPipeline();

app.Run();