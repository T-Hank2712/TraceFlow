using TraceFlow.Api.Domain.Common;
using TraceFlow.Api.Domain.Entities;

namespace TraceFlow.Api.Domain.Entities;
public class RefreshToken : Entity
{
    public Ulid UserId { get; private set; }
    public User? User { get; set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    public bool IsRevoked => RevokedAt is not null;
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;
    private RefreshToken() {}

    public RefreshToken(Ulid UserId,string TokenHash, DateTime ExpiresAt)
    {
        this.UserId = UserId;
        this.TokenHash = TokenHash;
        this.ExpiresAt = ExpiresAt;
        this.CreatedAt = DateTime.UtcNow;
        this.UpdatedAt = DateTime.UtcNow;
    }
    public void Revoke()
    {
        this.RevokedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}