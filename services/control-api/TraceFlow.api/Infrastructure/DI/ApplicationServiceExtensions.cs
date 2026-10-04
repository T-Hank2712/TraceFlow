namespace TraceFlow.Api.Infrastructure.DependencyInjection;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        services.AddControllers();

        services.AddValidatorsFromAssemblyContaining<Program>();

        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(
                typeof(Program).Assembly);
        });

        services.AddScoped<PasswordHasher>();
        services.AddScoped<JwtTokenGenerator>();
        services.AddScoped<RefreshTokenGenerator>();
        services.AddScoped<WorkspaceAccessService>();
        services.AddScoped<ProjectAccessService>();
        services.AddScoped<UserLookupService>();
        services.AddScoped<ApiKeyGenerator>();
        services.AddScoped<ApiKeyHasher>();
        services.AddScoped<ApiKeyExpirationPolicyResolver>();
        services.AddScoped<ApiKeyParser>();
        services.AddScoped<InvitationExpirationService>();

        services.AddSingleton(TimeProvider.System);

        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(ValidationBehavior<,>));

        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();

        return services;
    }
}
