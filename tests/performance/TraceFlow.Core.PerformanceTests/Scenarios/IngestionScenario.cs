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