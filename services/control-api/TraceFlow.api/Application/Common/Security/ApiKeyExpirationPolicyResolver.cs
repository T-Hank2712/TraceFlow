using TraceFlow.Api.Domain.Constants;

namespace TraceFlow.Api.Application.Common.Security;

public class ApiKeyExpirationPolicyResolver
{
    public DateTime? Resolve(int policy)
    {
        var now = DateTime.UtcNow;

        return policy switch
        {
            ApiKeyExpirationPolicies.OneMonth => now.AddMonths(1),
            ApiKeyExpirationPolicies.ThreeMonths => now.AddMonths(3),
            ApiKeyExpirationPolicies.NineMonths => now.AddMonths(9),
            ApiKeyExpirationPolicies.TwelveMonths => now.AddMonths(12),
            _ => throw new InvalidOperationException("Invalid API key expiration policy.")
        };
    }
}