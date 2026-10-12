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

        var databaseOptions = configuration
            .GetSection(DatabaseOptions.SectionName)
            .Get<DatabaseOptions>() ?? throw new InvalidOperationException("Database options are not configured.");

        services.AddDbContextPool<AppDbContext>(
            options =>
                options.UseNpgsql(
                    postgresConnectionString,
                    npgsqlOptions =>
                    {
                        npgsqlOptions.EnableRetryOnFailure(
                            maxRetryCount: databaseOptions.MaxRetryCount,
                            maxRetryDelay: TimeSpan.FromSeconds(databaseOptions.MaxRetryDelaySeconds),
                            errorCodesToAdd: null
                        );
                    }));

        services
            .AddHealthChecks()
            .AddNpgSql(
                postgresConnectionString,
                name: "postgres");

        return services;
    }
}
