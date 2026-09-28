using NBomber.CSharp;
using TraceFlow.Core.PerformanceTests.Configuration;
using TraceFlow.Core.PerformanceTests.Scenarios;

var options = new PerformanceOptions
{
    BaseUrl = GetEnvironmentVariable(
        "TRACEFLOW_BASE_URL",
        "http://localhost:5100"),

    ApiKey = GetRequiredEnvironmentVariable(
        "TRACEFLOW_API_KEY"),

    Rate = GetIntEnvironmentVariable(
        "TRACEFLOW_RATE",
        10),

    DurationSeconds = GetIntEnvironmentVariable(
        "TRACEFLOW_DURATION_SECONDS",
        30)
};

Console.WriteLine();
Console.WriteLine("========================================");
Console.WriteLine("TraceFlow Performance Test");
Console.WriteLine("========================================");
Console.WriteLine($"Target   : {options.BaseUrl}");
Console.WriteLine($"Rate     : {options.Rate} req/s");
Console.WriteLine($"Duration : {options.DurationSeconds}s");
Console.WriteLine("========================================");
Console.WriteLine();

var scenario = IngestionScenario.Create(options);

NBomberRunner
    .RegisterScenarios(scenario)
    .Run();

static string GetEnvironmentVariable(
    string name,
    string defaultValue)
{
    return Environment.GetEnvironmentVariable(name)
        ?? defaultValue;
}

static string GetRequiredEnvironmentVariable(
    string name)
{
    var value = Environment.GetEnvironmentVariable(name);

    if (string.IsNullOrWhiteSpace(value))
    {
        throw new InvalidOperationException(
            $"Environment variable '{name}' is required.");
    }

    return value;
}

static int GetIntEnvironmentVariable(
    string name,
    int defaultValue)
{
    var value = Environment.GetEnvironmentVariable(name);

    return int.TryParse(value, out var result)
        ? result
        : defaultValue;
}