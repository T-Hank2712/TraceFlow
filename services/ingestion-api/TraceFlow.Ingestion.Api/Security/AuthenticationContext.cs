using TraceFlow.Ingestion.Api.Contracts.Authentication;

namespace TraceFlow.Ingestion.Api.Security;

public static class AuthenticationContext
{
    private static readonly object Key = new();

    public static AuthenticatedContext? Get(
        HttpContext context)
    {
        return context.Items.TryGetValue(Key, out var value)
            ? value as AuthenticatedContext
            : null;
    }

    public static void Set(
        HttpContext context,
        AuthenticatedContext value)
    {
        context.Items[Key] = value;
    }
}