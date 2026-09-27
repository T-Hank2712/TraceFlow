using FluentAssertions;
using TraceFlow.Api.Domain.Entities;

namespace TraceFlow.Api.UnitTests.Domain.Entities;

public class RefreshTokenTests
{
    [Fact]
    public void Constructor_Should_Create_RefreshToken()
    {
        // Arrange
        var userId = Ulid.NewUlid();
        const string tokenHash = "hashed-refresh-token";
        var expiresAt = DateTime.UtcNow.AddDays(30);

        // Act
        var refreshToken = new RefreshToken(
            userId,
            tokenHash,
            expiresAt);

        // Assert
        refreshToken.Id.Should().NotBe(Ulid.Empty);
        refreshToken.UserId.Should().Be(userId);
        refreshToken.TokenHash.Should().Be(tokenHash);
        refreshToken.ExpiresAt.Should().Be(expiresAt);
        refreshToken.RevokedAt.Should().BeNull();

        refreshToken.IsRevoked.Should().BeFalse();
        refreshToken.IsExpired.Should().BeFalse();
        refreshToken.IsActive.Should().BeTrue();

        refreshToken.CreatedAt.Should().BeCloseTo(
            DateTime.UtcNow,
            TimeSpan.FromSeconds(2));

        refreshToken.UpdatedAt.Should().BeCloseTo(
            DateTime.UtcNow,
            TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void IsRevoked_Should_Return_False_When_Token_Has_Not_Been_Revoked()
    {
        // Arrange
        var refreshToken = CreateRefreshToken(
            DateTime.UtcNow.AddDays(30));

        // Assert
        refreshToken.IsRevoked.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_Should_Return_False_When_Expiration_Is_In_Future()
    {
        // Arrange
        var refreshToken = CreateRefreshToken(
            DateTime.UtcNow.AddMinutes(10));

        // Assert
        refreshToken.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_Should_Return_True_When_Expiration_Is_In_Past()
    {
        // Arrange
        var refreshToken = CreateRefreshToken(
            DateTime.UtcNow.AddMinutes(-1));

        // Assert
        refreshToken.IsExpired.Should().BeTrue();
    }

    [Fact]
    public void IsExpired_Should_Return_True_When_Expiration_Equals_Current_Time()
    {
        // Arrange
        var expiresAt = DateTime.UtcNow;
        var refreshToken = CreateRefreshToken(expiresAt);

        // Act
        var result = refreshToken.IsExpired;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsActive_Should_Return_True_When_Token_Is_Not_Revoked_And_Not_Expired()
    {
        // Arrange
        var refreshToken = CreateRefreshToken(
            DateTime.UtcNow.AddDays(30));

        // Assert
        refreshToken.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Revoke_Should_Set_RevokedAt()
    {
        // Arrange
        var refreshToken = CreateRefreshToken(
            DateTime.UtcNow.AddDays(30));

        var before = DateTime.UtcNow;

        // Act
        refreshToken.Revoke();

        var after = DateTime.UtcNow;

        // Assert
        refreshToken.RevokedAt.Should().NotBeNull();
        refreshToken.RevokedAt!.Value.Should().BeOnOrAfter(before);
        refreshToken.RevokedAt.Value.Should().BeOnOrBefore(after);
    }

    [Fact]
    public void Revoke_Should_Set_IsRevoked_To_True()
    {
        // Arrange
        var refreshToken = CreateRefreshToken(
            DateTime.UtcNow.AddDays(30));

        // Act
        refreshToken.Revoke();

        // Assert
        refreshToken.IsRevoked.Should().BeTrue();
    }

    [Fact]
    public void Revoke_Should_Set_IsActive_To_False()
    {
        // Arrange
        var refreshToken = CreateRefreshToken(
            DateTime.UtcNow.AddDays(30));

        // Act
        refreshToken.Revoke();

        // Assert
        refreshToken.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Revoke_Should_Update_UpdatedAt()
    {
        // Arrange
        var refreshToken = CreateRefreshToken(
            DateTime.UtcNow.AddDays(30));

        var previousUpdatedAt = refreshToken.UpdatedAt;

        // Act
        refreshToken.Revoke();

        // Assert
        refreshToken.UpdatedAt.Should().BeOnOrAfter(previousUpdatedAt);
    }

    [Fact]
    public void IsActive_Should_Return_False_When_Token_Is_Expired()
    {
        // Arrange
        var refreshToken = CreateRefreshToken(
            DateTime.UtcNow.AddMinutes(-1));

        // Assert
        refreshToken.IsExpired.Should().BeTrue();
        refreshToken.IsRevoked.Should().BeFalse();
        refreshToken.IsActive.Should().BeFalse();
    }

    [Fact]
    public void IsActive_Should_Return_False_When_Token_Is_Revoked()
    {
        // Arrange
        var refreshToken = CreateRefreshToken(
            DateTime.UtcNow.AddDays(30));

        refreshToken.Revoke();

        // Assert
        refreshToken.IsRevoked.Should().BeTrue();
        refreshToken.IsExpired.Should().BeFalse();
        refreshToken.IsActive.Should().BeFalse();
    }

    private static RefreshToken CreateRefreshToken(
        DateTime expiresAt)
    {
        return new RefreshToken(
            Ulid.NewUlid(),
            "hashed-refresh-token",
            expiresAt);
    }
}