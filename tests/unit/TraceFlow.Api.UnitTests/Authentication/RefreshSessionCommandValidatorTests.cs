using FluentAssertions;
using TraceFlow.Api.Application.Auth.Commands.RefreshSession;

namespace TraceFlow.Api.UnitTests.Authentication;

public class RefreshSessionCommandValidatorTests
{
    private readonly RefreshSessionCommandValidator _validator;

    public RefreshSessionCommandValidatorTests()
    {
        _validator = new RefreshSessionCommandValidator();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_be_invalid_when_refresh_token_is_empty(
        string refreshToken)
    {
        // Arrange
        var command = new RefreshSessionCommand(refreshToken);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x =>
            x.PropertyName == nameof(RefreshSessionCommand.RefreshToken));
    }

    [Theory]
    [InlineData("refresh-token")]
    [InlineData("valid-refresh-token")]
    [InlineData("some-random-refresh-token")]
    public void Should_be_valid_when_refresh_token_is_provided(
        string refreshToken)
    {
        // Arrange
        var command = new RefreshSessionCommand(refreshToken);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
