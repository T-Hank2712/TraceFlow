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
                            service = "order-service",
                            level = 3,
                            message =
                                $"Order request processed successfully. " +
                                $"EventId={Guid.NewGuid()}",

                            timestamp = DateTime.UtcNow,

                            traceId = Guid.NewGuid().ToString("N"),
                            correlationId = Guid.NewGuid().ToString("N"),

                            metadata = new
                            {
                                source = "nbomber",
                                environment = "performance",

                                host = "traceflow-performance-client",

                                http = new
                                {
                                    method = "POST",
                                    route = "/api/orders",
                                    statusCode = 200,
                                    durationMs = 42
                                },

                                request = new
                                {
                                    requestId = Guid.NewGuid().ToString("N"),
                                    userId = "user-10001",
                                    clientIp = "127.0.0.1"
                                },

                                application = new
                                {
                                    name = "order-service",
                                    version = "2.4.1",
                                    instance = "order-service-01"
                                },

                                business = new
                                {
                                    orderId = $"ORD-{Random.Shared.Next(100000, 999999)}",
                                    amount = 125.50,
                                    currency = "USD"
                                }
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