using Microsoft.AspNetCore.Identity; 

namespace TraceFlow.Api.Application.Common.Security;
public class PasswordHasher
{
    private readonly PasswordHasher<object> _hasher = new();
    public string Hash(string password)
    {
        return _hasher.HashPassword(null!,password);
    }
    public bool Verify(string password, string passwordHash) 
    { 
        var result = _hasher.VerifyHashedPassword( null!, passwordHash, password); 
        return result == PasswordVerificationResult.Success || 
                result == PasswordVerificationResult.SuccessRehashNeeded; 
    }
}