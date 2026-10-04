namespace TraceFlow.Api.Infrastructure.DependencyInjection;

public static class ConfigurationExtensions
{
    public static IConfigurationBuilder AddEnvironmentFile(
        this IConfigurationBuilder configuration,
        IWebHostEnvironment environment)
    {
        var envPath = Path.Combine(
            environment.ContentRootPath,
            ".env");

        if (File.Exists(envPath))
        {
            Env.Load(envPath);
            configuration.AddEnvironmentVariables();
        }

        return configuration;
    }
}