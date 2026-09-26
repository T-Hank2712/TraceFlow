using TraceFlow.Ingestion.Api.Security;

namespace TraceFlow.Ingestion.Api.Middleware;

public sealed class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    public AuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        Authenticator authenticator)
    {
        var result = await authenticator.AuthenticateAsync(
            context.Request.Headers.Authorization,
            context.RequestAborted);

        if (!result.Success)
        {
            context.Response.StatusCode = result.StatusCode;
            await context.Response.WriteAsJsonAsync(result);

            return;
        }

        AuthenticationContext.Set(
            context,
            result.Data!);

        await _next(context);
    }
}