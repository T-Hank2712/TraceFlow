using FluentAssertions;
using TraceFlow.Api.Application.Common.Security;
using Xunit;

namespace TraceFlow.Api.UnitTests.Common.Security;

public class PasswordHasherTests
{
    private readonly PasswordHasher _passwordHasher;

    public PasswordHasherTests()
    {
        _passwordHasher = new PasswordHasher();
    }

    [Fact]
    public void Hash_Should_Return_HashedPassword()
    {
        // Arrange
        const string password = "Password123!";

        // Act
        var hash = _passwordHasher.Hash(password);

        // Assert
        hash.Should().NotBeNullOrWhiteSpace();
        hash.Should().NotBe(password);
    }

    [Theory]
    [InlineData("Password123!")]
    [InlineData("AnotherPassword456@")]
    [InlineData("TestPassword789#")]
    public void Verify_Should_Return_True_When_Password_Is_Correct(
        string password)
    {
        // Arrange
        var hash = _passwordHasher.Hash(password);

        // Act
        var result = _passwordHasher.Verify(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("WrongPassword123!")]
    [InlineData("Password123")]
    [InlineData("password123!")]
    public void Verify_Should_Return_False_When_Password_Is_Incorrect(
        string password)
    {
        // Arrange
        const string originalPassword = "Password123!";
        var hash = _passwordHasher.Hash(originalPassword);

        // Act
        var result = _passwordHasher.Verify(password, hash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Hash_Should_Produce_Different_Hash_For_Same_Password()
    {
        // Arrange
        const string password = "Password123!";

        // Act
        var firstHash = _passwordHasher.Hash(password);
        var secondHash = _passwordHasher.Hash(password);

        // Assert
        firstHash.Should().NotBe(secondHash);
    }
}