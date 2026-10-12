namespace TraceFlow.Api.Domain.Entities;

public class RefreshToken : Entity
{
    public Ulid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public string TokenHash { get; private set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }

    public bool IsRevoked => RevokedAt is not null;
    public bool IsExpired(DateTimeOffset now) => now >= ExpiresAt;
    public bool IsActive(DateTimeOffset now) => !IsRevoked && !IsExpired(now);
    private RefreshToken() { }

    public RefreshToken(Ulid UserId, string TokenHash, DateTimeOffset ExpiresAt, DateTimeOffset createdAt)
    {
        this.UserId = UserId;
        this.TokenHash = TokenHash;
        this.ExpiresAt = ExpiresAt;
        this.CreatedAt = createdAt;
        this.UpdatedAt = createdAt;
    }
    public void Revoke(DateTimeOffset revokedAt)
    {
        if (IsRevoked)
        {
            return;
        }

        RevokedAt = revokedAt;
        UpdatedAt = revokedAt; // Đồng bộ mốc thời gian thu hồi
    }
}
