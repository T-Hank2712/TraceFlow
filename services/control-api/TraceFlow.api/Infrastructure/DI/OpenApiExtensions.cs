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

                                AddProblemDetailsResponses(operation);
                            }
                        }

                        return Task.CompletedTask;
                    });
            });

        return services;
    }
    private static void AddProblemDetailsResponses(OpenApiOperation operation)
    {
        AddProblemDetailsResponse(
            operation,
            StatusCodes.Status400BadRequest,
            "Bad Request");

        AddProblemDetailsResponse(
            operation,
            StatusCodes.Status401Unauthorized,
            "Unauthorized");

        AddProblemDetailsResponse(
            operation,
            StatusCodes.Status403Forbidden,
            "Forbidden");

        AddProblemDetailsResponse(
            operation,
            StatusCodes.Status404NotFound,
            "Not Found");

        AddProblemDetailsResponse(
            operation,
            StatusCodes.Status409Conflict,
            "Conflict");

        AddProblemDetailsResponse(
            operation,
            StatusCodes.Status500InternalServerError,
            "Internal Server Error");
    }

    private static void AddProblemDetailsResponse(
        OpenApiOperation operation,
        int statusCode,
        string description)
    {
        var statusCodeText = statusCode.ToString();

        operation.Responses ??= new OpenApiResponses();

        if (operation.Responses.ContainsKey(statusCodeText))
        {
            return;
        }

        operation.Responses[statusCodeText] = new OpenApiResponse
        {
            Description = description,
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/problem+json"] = new()
                {
                    Schema = CreateProblemDetailsSchema()
                }
            }
        };
    }

    private static OpenApiSchema CreateProblemDetailsSchema()
    {
        return new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            Properties = new Dictionary<string, IOpenApiSchema>
            {
                ["type"] = new OpenApiSchema
                {
                    Type = JsonSchemaType.String
                },
                ["title"] = new OpenApiSchema
                {
                    Type = JsonSchemaType.String
                },
                ["status"] = new OpenApiSchema
                {
                    Type = JsonSchemaType.Integer,
                    Format = "int32"
                },
                ["detail"] = new OpenApiSchema
                {
                    Type = JsonSchemaType.String
                },
                ["instance"] = new OpenApiSchema
                {
                    Type = JsonSchemaType.String
                },
                ["traceId"] = new OpenApiSchema
                {
                    Type = JsonSchemaType.String
                }
            }
        };
    }
}