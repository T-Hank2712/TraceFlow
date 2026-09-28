using Microsoft.FSharp.Core;
using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Http.CSharp;
using TraceFlow.Core.PerformanceTests.Configuration;

namespace TraceFlow.Core.PerformanceTests.Scenarios;

public static class IngestionScenario
{
    public static ScenarioProps Create(PerformanceOptions options)
    {
        var httpClient = Http.CreateDefaultClient();

        return Scenario.Create(
                "ingestion_single_log",
                async context =>
                {
                    var payload = new
                    {
                        service = "traceflow-performance-test",
                        level = 3,
                        message = $"Performance test event {Guid.NewGuid()}",
                        timestamp = DateTime.UtcNow,
                        metadata = new
                        {
                            source = "nbomber"
                        }
                    };

                    var request = Http.CreateRequest(
                            "POST",
                            $"{options.BaseUrl}/logs")
                        .WithHeader(
                            "Authorization",
                            $"ApiKey {options.ApiKey}")
                        .WithHeader(
                            "Accept",
                            "application/json")
                        .WithJsonBody(payload);

                    var response = await Http.Send(
                        httpClient,
                        request);

                    var httpResponse =
                        FSharpOption<HttpResponseMessage>.get_IsSome(
                            response.Payload)
                            ? response.Payload.Value
                            : null;

                    if (httpResponse is null)
                    {
                        return Response.Fail();
                    }

                    return httpResponse.IsSuccessStatusCode
                        ? Response.Ok()
                        : Response.Fail();
                })
            .WithoutWarmUp()
            .WithLoadSimulations(
                Simulation.Inject(
                    rate: options.Rate,
                    interval: TimeSpan.FromSeconds(1),
                    during: TimeSpan.FromSeconds(
                        options.DurationSeconds)));
    }
}