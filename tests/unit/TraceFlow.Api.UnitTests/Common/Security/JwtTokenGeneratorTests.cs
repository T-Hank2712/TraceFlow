using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using TraceFlow.Api.Application.Common.Security;
using TraceFlow.Api.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace TraceFlow.Api.UnitTests.Common.Security;

public class JwtTokenGeneratorTests
{
    private const string Issuer = "TraceFlow";
    private const string Audience = "TraceFlow";
    private const string Secret =
        "this-is-a-test-secret-key-with-at-least-32-characters";

    private readonly JwtTokenGenerator _generator;

    public JwtTokenGeneratorTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = Issuer,
                ["Jwt:Audience"] = Audience,
                ["Jwt:Secret"] = Secret,
                ["Jwt:AccessTokenExpirationMinutes"] = "15"
            })
            .Build();

        _generator = new JwtTokenGenerator(configuration);
    }

    [Fact]
    public void Generate_Should_Return_Token()
    {
        // Arrange
        var user = CreateUser();

        // Act
        var result = _generator.Generate(user);

        // Assert
        result.Token.Should().NotBeNullOrWhiteSpace();
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void Generate_Should_Create_Valid_Jwt_Token()
    {
        // Arrange
        var user = CreateUser();

        // Act
        var result = _generator.Generate(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(result.Token);

        token.Should().NotBeNull();
        token.RawData.Should().Be(result.Token);
    }

    [Fact]
    public void Generate_Should_Set_Correct_Claims()
    {
        // Arrange
        var user = CreateUser();

        // Act
        var result = _generator.Generate(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(result.Token);

        token.Claims.Should().ContainSingle(x =>
            x.Type == ClaimTypes.NameIdentifier &&
            x.Value == user.Id.ToString());

        token.Claims.Should().ContainSingle(x =>
            x.Type == JwtRegisteredClaimNames.Sub &&
            x.Value == user.Id.ToString());

        token.Claims.Should().ContainSingle(x =>
            x.Type == JwtRegisteredClaimNames.Email &&
            x.Value == user.Email);

        token.Claims.Should().ContainSingle(x =>
            x.Type == "username" &&
            x.Value == user.UserName);

        token.Claims.Should().ContainSingle(x =>
            x.Type == ClaimTypes.Role &&
            x.Value == user.Role);
    }

    [Fact]
    public void Generate_Should_Set_Correct_Issuer()
    {
        // Arrange
        var user = CreateUser();

        // Act
        var result = _generator.Generate(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(result.Token);

        token.Issuer.Should().Be(Issuer);
    }

    [Fact]
    public void Generate_Should_Set_Correct_Audience()
    {
        // Arrange
        var user = CreateUser();

        // Act
        var result = _generator.Generate(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(result.Token);

        token.Audiences.Should().ContainSingle(Audience);
    }

    [Fact]
    public void Generate_Should_Set_Expiration_According_To_Configuration()
    {
        // Arrange
        var user = CreateUser();

        var before = DateTime.UtcNow;

        // Act
        var result = _generator.Generate(user);

        var after = DateTime.UtcNow;

        // Assert
        result.ExpiresAt.Should().BeOnOrAfter(
            before.AddMinutes(15));

        result.ExpiresAt.Should().BeOnOrBefore(
            after.AddMinutes(15));
    }

    [Fact]
    public void Generate_Should_Use_HmacSha256_Signing_Algorithm()
    {
        // Arrange
        var user = CreateUser();

        // Act
        var result = _generator.Generate(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(result.Token);

        token.Header.Alg.Should().Be(SecurityAlgorithms.HmacSha256);
    }

    private static User CreateUser()
    {
        return new User(
            Email: "john.doe@example.com",
            UserName: "john.doe",
            FirstName: "John",
            LastName: "Doe",
            PasswordHash: "hashed-password");
    }
}