using FluentAssertions;
using Microsoft.Extensions.Configuration;
using TraceFlow.Api.Application.Common.Security;
using Microsoft.Extensions.Options;
using TraceFlow.Api.Infrastructure.Options;

namespace TraceFlow.Api.UnitTests.Common.Security;

public class ApiKeyHasherTests
{
    private const string Pepper = "test-api-key-pepper";

    private readonly ApiKeyHasher _hasher;

    public ApiKeyHasherTests()
    {
        var options = Options.Create(new ApiKeySecurityOptions
        {
            Pepper = Pepper
        });

       _hasher = new ApiKeyHasher(options);
    }

    [Fact]
    public void Hash_Should_Return_Hash()
    {
        // Arrange
        var secret = "tfk_live_test_secret";

        // Act
        var result = _hasher.Hash(secret);

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Hash_Should_Return_Lowercase_Hex_String()
    {
        // Arrange
        var secret = "tfk_live_test_secret";

        // Act
        var result = _hasher.Hash(secret);

        // Assert
        result.Should().HaveLength(64);
        result.Should().MatchRegex("^[0-9a-f]{64}$");
    }

    [Theory]
    [InlineData("secret-1")]
    [InlineData("secret-2")]
    [InlineData("tfk_live_01HXYZ_test")]
    public void Hash_Should_Return_Same_Hash_For_Same_Secret(
        string secret)
    {
        // Act
        var firstHash = _hasher.Hash(secret);
        var secondHash = _hasher.Hash(secret);

        // Assert
        firstHash.Should().Be(secondHash);
    }

    [Fact]
    public void Hash_Should_Return_Different_Hash_For_Different_Secrets()
    {
        // Arrange
        var firstSecret = "tfk_live_secret_1";
        var secondSecret = "tfk_live_secret_2";

        // Act
        var firstHash = _hasher.Hash(firstSecret);
        var secondHash = _hasher.Hash(secondSecret);

        // Assert
        firstHash.Should().NotBe(secondHash);
    }

    [Theory]
    [InlineData("tfk_live_secret")]
    [InlineData("another-secret")]
    [InlineData("test-api-key-123")]
    public void Verify_Should_Return_True_When_Secret_Matches_Hash(
        string secret)
    {
        // Arrange
        var hash = _hasher.Hash(secret);

        // Act
        var result = _hasher.Verify(secret, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Verify_Should_Return_False_When_Secret_Does_Not_Match_Hash()
    {
        // Arrange
        var secret = "tfk_live_correct_secret";
        var wrongSecret = "tfk_live_wrong_secret";
        var hash = _hasher.Hash(secret);

        // Act
        var result = _hasher.Verify(wrongSecret, hash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Verify_Should_Return_False_When_Hash_Is_Invalid()
    {
        // Arrange
        var secret = "tfk_live_secret";
        var invalidHash = new string('a', 64);

        // Act
        var result = _hasher.Verify(secret, invalidHash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Hash_Should_Throw_When_Pepper_Is_Not_Configured()
    {
        // Arrange
        var options = Options.Create(new ApiKeySecurityOptions
        {
            Pepper = string.Empty
        });

        var hasher = new ApiKeyHasher(options);

        // Act
        var act = () => hasher.Hash("tfk_live_secret");

        // Assert
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("API key pepper is not configured.");
    }

    [Fact]
    public void Verify_Should_Throw_When_Pepper_Is_Not_Configured()
    {
        // Arrange
        var options = Options.Create(new ApiKeySecurityOptions
        {
            Pepper = string.Empty
        });

        var hasher = new ApiKeyHasher(options);

        // Act
        var act = () => hasher.Verify(
            "tfk_live_secret",
            new string('a', 64));

        // Assert
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("API key pepper is not configured.");
    }
}