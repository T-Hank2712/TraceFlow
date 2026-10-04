namespace TraceFlow.Api.Infrastructure.DependencyInjection;

public static class PersistenceExtensions
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var postgresConnectionString =
            configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException(
                "Postgres connection string is not configured.");

        services.AddDbContextPool<AppDbContext>(
            options =>
                options.UseNpgsql(postgresConnectionString));

        services
            .AddHealthChecks()
            .AddNpgSql(
                postgresConnectionString,
                name: "postgres");

        return services;
    }
}
