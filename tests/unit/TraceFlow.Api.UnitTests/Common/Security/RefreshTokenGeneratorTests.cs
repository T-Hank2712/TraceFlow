using FluentAssertions;
using TraceFlow.Api.Application.Common.Security;

namespace TraceFlow.Api.UnitTests.Common.Security;

public class RefreshTokenGeneratorTests
{
    [Fact]
    public void Generate_Should_Return_Token_And_Hash()
    {
        // Act
        var result = new RefreshTokenGenerator().Generate();

        // Assert
        result.Token.Should().NotBeNullOrWhiteSpace();
        result.Hash.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Generate_Should_Return_Token_With_Expected_Length()
    {
        // Act
        var result = new RefreshTokenGenerator().Generate();

        // Assert
        result.Token.Should().HaveLength(88);
    }

    [Fact]
    public void Generate_Should_Return_Sha256_Hash()
    {
        // Act
        var result = new RefreshTokenGenerator().Generate();

        // Assert
        result.Hash.Should().HaveLength(64);
        result.Hash.Should().MatchRegex("^[0-9A-F]+$");
    }

    [Fact]
    public void Generate_Should_Return_Hash_Matching_Token()
    {
        // Act
        var result = new RefreshTokenGenerator().Generate();

        // Assert
        RefreshTokenGenerator.Hash(result.Token)
            .Should()
            .Be(result.Hash);
    }

    [Fact]
    public void Generate_Should_Produce_Different_Tokens()
    {
        // Act
        var first = RefreshTokenGenerator.GenerateRawToken();
        var second = RefreshTokenGenerator.GenerateRawToken();

        // Assert
        first.Should().NotBe(second);
    }

    [Fact]
    public void Hash_Should_Return_Deterministic_Hash()
    {
        // Arrange
        const string token = "test-refresh-token";

        // Act
        var firstHash = RefreshTokenGenerator.Hash(token);
        var secondHash = RefreshTokenGenerator.Hash(token);

        // Assert
        firstHash.Should().Be(secondHash);
    }

    [Theory]
    [InlineData("test-refresh-token")]
    [InlineData("another-refresh-token")]
    [InlineData("Password123!")]
    [InlineData("1234567890")]
    public void Hash_Should_Return_Consistent_Hash_For_Same_Input(
        string token)
    {
        // Act
        var hash = RefreshTokenGenerator.Hash(token);

        // Assert
        hash.Should().Be(RefreshTokenGenerator.Hash(token));
    }

    [Theory]
    [InlineData("token-a", "token-b")]
    [InlineData("refresh-token-1", "refresh-token-2")]
    [InlineData("abc", "abcd")]
    public void Hash_Should_Return_Different_Hash_For_Different_Input(
        string firstToken,
        string secondToken)
    {
        // Act
        var firstHash = RefreshTokenGenerator.Hash(firstToken);
        var secondHash = RefreshTokenGenerator.Hash(secondToken);

        // Assert
        firstHash.Should().NotBe(secondHash);
    }
}
