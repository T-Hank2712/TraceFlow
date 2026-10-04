namespace TraceFlow.Api.Infrastructure.DependencyInjection;

public static class OpenApiExtensions
{
    public static IServiceCollection AddApiDocumentation(
        this IServiceCollection services)
    {
        services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = false;
                options.ReportApiVersions = true;
                options.ApiVersionReader =
                    new UrlSegmentApiVersionReader();
            })
            .AddMvc()
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            })
            .AddOpenApi(options =>
            {
                options.Document.AddDocumentTransformer(
                    (document, context, cancellationToken) =>
                    {
                        document.Servers =
                        [
                            new OpenApiServer
                            {
                                Url = "/control"
                            }
                        ];

                        document.Components ??=
                            new OpenApiComponents();

                        document.Components.SecuritySchemes ??=
                            new Dictionary<string, IOpenApiSecurityScheme>();

                        document.Components.SecuritySchemes["Bearer"] =
                            new OpenApiSecurityScheme
                            {
                                Type = SecuritySchemeType.Http,
                                Scheme = "bearer",
                                BearerFormat = "JWT",
                                Name = "Authorization",
                                In = ParameterLocation.Header,
                                Description =
                                    "Enter JWT access token only. Swagger UI will add the Bearer prefix."
                            };

                        foreach (var path in document.Paths.Values)
                        {
                            if (path.Operations is null)
                            {
                                continue;
                            }

                            foreach (var operation in path.Operations.Values)
                            {
                                operation.Security ??= [];

                                operation.Security.Add(
                                    new OpenApiSecurityRequirement
                                    {
                                        [
                                            new OpenApiSecuritySchemeReference(
                                                "Bearer",
                                                document)
                                        ] = []
                                    });
                            }
                        }

                        return Task.CompletedTask;
                    });
            });

        return services;
    }
}