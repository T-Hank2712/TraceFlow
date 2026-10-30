using FluentAssertions;
using TraceFlow.Api.Domain.Constants;
using TraceFlow.Api.Domain.Entities;

namespace TraceFlow.Api.UnitTests.Domain.Entities;

public class ApiKeyTests
{
    [Fact]
    public void Constructor_Should_Create_Active_ApiKey()
    {
        // Arrange
        var apiKeyId = Ulid.NewUlid();
        var traceApplicationId = Ulid.NewUlid();
        var expiresAt = DateTime.UtcNow.AddMonths(3);

        // Act
        var apiKey = new ApiKey(
            apiKeyId,
            traceApplicationId,
            "  Production API Key  ",
            "  PRODUCTION  ",
            "  tfk_live_abc123  ",
            "secret-hash",
            expiresAt);

        // Assert
        apiKey.Id.Should().Be(apiKeyId);
        apiKey.TraceApplicationId.Should().Be(traceApplicationId);
        apiKey.Name.Should().Be("Production API Key");
        apiKey.Environment.Should().Be("production");
        apiKey.KeyPrefix.Should().Be("tfk_live_abc123");
        apiKey.SecretHash.Should().Be("secret-hash");
        apiKey.Status.Should().Be(ApiKeyStatuses.Active);
        apiKey.ExpiresAt.Should().Be(expiresAt);
        apiKey.RevokedAt.Should().BeNull();
        apiKey.LastUsedAt.Should().BeNull();

        apiKey.CreatedAt.Should().BeCloseTo(
            DateTime.UtcNow,
            TimeSpan.FromSeconds(2));

        apiKey.UpdatedAt.Should().BeCloseTo(
            DateTime.UtcNow,
            TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Constructor_Should_Allow_Null_Expiration()
    {
        // Act
        var apiKey = CreateApiKey(expiresAt: null);

        // Assert
        apiKey.ExpiresAt.Should().BeNull();
        apiKey.IsExpired(DateTime.UtcNow).Should().BeFalse();
        apiKey.IsUsable(DateTime.UtcNow).Should().BeTrue();
    }

    [Fact]
    public void Revoke_Should_Set_Status_To_Revoked()
    {
        // Arrange
        var apiKey = CreateApiKey();

        // Act
        apiKey.Revoke();

        // Assert
        apiKey.Status.Should().Be(ApiKeyStatuses.Revoked);
        apiKey.RevokedAt.Should().NotBeNull();
    }

    [Fact]
    public void Revoke_Should_Set_RevokedAt_To_Current_Utc_Time()
    {
        // Arrange
        var apiKey = CreateApiKey();
        var before = DateTime.UtcNow;

        // Act
        apiKey.Revoke();

        var after = DateTime.UtcNow;

        // Assert
        apiKey.RevokedAt.Should().NotBeNull();
        apiKey.RevokedAt!.Value.Should().BeOnOrAfter(before);
        apiKey.RevokedAt.Value.Should().BeOnOrBefore(after);
    }

    [Fact]
    public void Revoke_Should_Update_UpdatedAt()
    {
        // Arrange
        var apiKey = CreateApiKey();
        var previousUpdatedAt = apiKey.UpdatedAt;

        // Act
        apiKey.Revoke();

        // Assert
        apiKey.UpdatedAt.Should().BeOnOrAfter(previousUpdatedAt);
    }

    [Fact]
    public void Revoke_Should_Be_Idempotent()
    {
        // Arrange
        var apiKey = CreateApiKey();

        apiKey.Revoke();

        var revokedAt = apiKey.RevokedAt;
        var updatedAt = apiKey.UpdatedAt;

        // Act
        apiKey.Revoke();

        // Assert
        apiKey.Status.Should().Be(ApiKeyStatuses.Revoked);
        apiKey.RevokedAt.Should().Be(revokedAt);
        apiKey.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public void MarkUsed_Should_Set_LastUsedAt()
    {
        // Arrange
        var apiKey = CreateApiKey();
        var before = DateTime.UtcNow;

        // Act
        apiKey.MarkUsed();

        var after = DateTime.UtcNow;

        // Assert
        apiKey.LastUsedAt.Should().NotBeNull();
        apiKey.LastUsedAt!.Value.Should().BeOnOrAfter(before);
        apiKey.LastUsedAt.Value.Should().BeOnOrBefore(after);
    }

    [Fact]
    public void MarkUsed_Should_Update_UpdatedAt()
    {
        // Arrange
        var apiKey = CreateApiKey();
        var previousUpdatedAt = apiKey.UpdatedAt;

        // Act
        apiKey.MarkUsed();

        // Assert
        apiKey.UpdatedAt.Should().BeOnOrAfter(previousUpdatedAt);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(60)]
    public void IsExpired_Should_Return_False_When_Expiration_Is_In_Future(
        int minutesFromNow)
    {
        // Arrange
        var utcNow = DateTime.UtcNow;
        var apiKey = CreateApiKey(
            expiresAt: utcNow.AddMinutes(minutesFromNow));

        // Act
        var result = apiKey.IsExpired(utcNow);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_Should_Return_True_When_Expiration_Is_In_Past()
    {
        // Arrange
        var utcNow = DateTime.UtcNow;
        var apiKey = CreateApiKey(
            expiresAt: utcNow.AddMinutes(-1));

        // Act
        var result = apiKey.IsExpired(utcNow);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsExpired_Should_Return_True_When_Expiration_Equals_Current_Time()
    {
        // Arrange
        var utcNow = DateTime.UtcNow;
        var apiKey = CreateApiKey(
            expiresAt: utcNow);

        // Act
        var result = apiKey.IsExpired(utcNow);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsExpired_Should_Return_False_When_Expiration_Is_Null()
    {
        // Arrange
        var utcNow = DateTime.UtcNow;
        var apiKey = CreateApiKey(expiresAt: null);

        // Act
        var result = apiKey.IsExpired(utcNow);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsUsable_Should_Return_True_When_Active_And_Not_Expired()
    {
        // Arrange
        var utcNow = DateTime.UtcNow;
        var apiKey = CreateApiKey(
            expiresAt: utcNow.AddMinutes(10));

        // Act
        var result = apiKey.IsUsable(utcNow);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsUsable_Should_Return_True_When_Active_And_Never_Expires()
    {
        // Arrange
        var utcNow = DateTime.UtcNow;
        var apiKey = CreateApiKey(expiresAt: null);

        // Act
        var result = apiKey.IsUsable(utcNow);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsUsable_Should_Return_False_When_Expired()
    {
        // Arrange
        var utcNow = DateTime.UtcNow;
        var apiKey = CreateApiKey(
            expiresAt: utcNow.AddMinutes(-1));

        // Act
        var result = apiKey.IsUsable(utcNow);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsUsable_Should_Return_False_When_Revoked()
    {
        // Arrange
        var utcNow = DateTime.UtcNow;
        var apiKey = CreateApiKey(
            expiresAt: utcNow.AddMinutes(10));

        apiKey.Revoke();

        // Act
        var result = apiKey.IsUsable(utcNow);

        // Assert
        result.Should().BeFalse();
    }

    private static ApiKey CreateApiKey(
        DateTime? expiresAt = null)
    {
        return new ApiKey(
            Ulid.NewUlid(),
            Ulid.NewUlid(),
            "Test API Key",
            "production",
            "tfk_live_test",
            "secret-hash",
            expiresAt);
    }
}