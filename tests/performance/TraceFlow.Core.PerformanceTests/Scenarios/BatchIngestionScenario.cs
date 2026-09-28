using Microsoft.FSharp.Core;
using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Http.CSharp;
using TraceFlow.Core.PerformanceTests.Configuration;

namespace TraceFlow.Core.PerformanceTests.Scenarios;

public static class BatchIngestionScenario
{
    public static ScenarioProps Create(
        PerformanceOptions options,
        int batchSize)
    {
        var httpClient = Http.CreateDefaultClient();

        return Scenario.Create(
                $"ingestion_batch_{batchSize}",
                async _ =>
                {
                    var logs = Enumerable
                        .Range(0, batchSize)
                        .Select(_ => new
                        {
                            service = "traceflow-performance-test",
                            level = 3,
                            message =
                                $"Performance test event {Guid.NewGuid()}",
                            metadata = new
                            {
                                source = "nbomber"
                            }
                        })
                        .ToArray();

                    var payload = new
                    {
                        logs
                    };

                    var request = Http.CreateRequest(
                            "POST",
                            $"{options.BaseUrl}/batch-logs")
                        .WithHeader(
                            "Authorization",
                            $"ApiKey {options.ApiKey}")
                        .WithHeader(
                            "Content-Type",
                            "application/json")
                        .WithJsonBody(payload);

                    var response = await Http.Send(
                        httpClient,
                        request);

                    if (FSharpOption<HttpResponseMessage>
                        .get_IsNone(response.Payload))
                    {
                        return Response.Fail();
                    }

                    var httpResponse =
                        response.Payload.Value;

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