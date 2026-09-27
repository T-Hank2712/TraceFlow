using FluentAssertions;
using TraceFlow.Api.Application.Common.Security;

namespace TraceFlow.Api.UnitTests.Common.Security;

public class ApiKeyGeneratorTests
{
    private readonly ApiKeyGenerator _generator;

    public ApiKeyGeneratorTests()
    {
        _generator = new ApiKeyGenerator();
    }

    [Fact]
    public void Generate_Should_Return_Secret_And_KeyPrefix()
    {
        // Arrange
        var apiKeyId = Ulid.NewUlid();

        // Act
        var result = _generator.Generate(apiKeyId);

        // Assert
        result.Secret.Should().NotBeNullOrWhiteSpace();
        result.KeyPrefix.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Generate_Should_Create_Secret_With_Correct_Format()
    {
        // Arrange
        var apiKeyId = Ulid.NewUlid();

        // Act
        var result = _generator.Generate(apiKeyId);

        // Assert
        result.Secret.Should().StartWith(
            $"tfk_live_{apiKeyId}_");

        result.Secret.Should().MatchRegex(
            $@"^tfk_live_{apiKeyId}_[A-Za-z0-9_-]+$");
    }

    [Fact]
    public void Generate_Should_Create_KeyPrefix_With_Correct_Format()
    {
        // Arrange
        var apiKeyId = Ulid.NewUlid();

        // Act
        var result = _generator.Generate(apiKeyId);

        // Assert
        var expectedPrefix = $"tfk_live_{apiKeyId}_";

        result.KeyPrefix.Should().StartWith(expectedPrefix);
        result.KeyPrefix.Should().HaveLength(
            expectedPrefix.Length + 8);
    }

    [Fact]
    public void Generate_Should_Use_Same_Secret_Prefix_In_KeyPrefix()
    {
        // Arrange
        var apiKeyId = Ulid.NewUlid();

        // Act
        var result = _generator.Generate(apiKeyId);

        // Assert
        var expectedPrefix = $"tfk_live_{apiKeyId}_";

        result.Secret.Should().StartWith(expectedPrefix);
        result.KeyPrefix.Should().StartWith(expectedPrefix);

        result.Secret[expectedPrefix.Length..]
            .Should()
            .StartWith(result.KeyPrefix[expectedPrefix.Length..]);
    }

    [Fact]
    public void Generate_Should_Produce_Different_Secrets()
    {
        // Arrange
        var apiKeyId = Ulid.NewUlid();

        // Act
        var first = _generator.Generate(apiKeyId);
        var second = _generator.Generate(apiKeyId);

        // Assert
        first.Secret.Should().NotBe(second.Secret);
        first.KeyPrefix.Should().NotBe(second.KeyPrefix);
    }
}
