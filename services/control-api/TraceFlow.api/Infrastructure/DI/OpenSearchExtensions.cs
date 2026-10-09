namespace TraceFlow.Api.Infrastructure.DependencyInjection;

public static class OpenSearchExtensions
{
    public static IServiceCollection AddOpenSearch(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var openSearchOptions =
            configuration
                .GetSection(OpenSearchOptions.SectionName)
                .Get<OpenSearchOptions>()
            ?? throw new InvalidOperationException(
                "OpenSearch options are not configured.");

        if (environment.IsProduction() && openSearchOptions.SkipTlsVerify)
        {
            throw new InvalidOperationException(
                "OpenSearch TLS verification cannot be disabled in production.");
        }

        services.AddHttpClient<
            ILogSearchReader,
            OpenSearchLogSearchReader>(client =>
        {
            client.BaseAddress = new Uri(openSearchOptions.Url);

            var credentials =
                Convert.ToBase64String(
                    Encoding.UTF8.GetBytes(
                        $"{openSearchOptions.Username}:{openSearchOptions.Password}"));

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Basic",
                    credentials);
        })
        .ConfigurePrimaryHttpMessageHandler(() =>
        {
            return new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    openSearchOptions.SkipTlsVerify
                        ? HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                        : null
            };
        });

        return services;
    }
}
