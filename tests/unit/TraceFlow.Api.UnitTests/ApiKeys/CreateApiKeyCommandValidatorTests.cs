using FluentAssertions;
using TraceFlow.Api.Application.ApiKeys.Commands.CreateApiKey;
using TraceFlow.Api.Domain.Constants;

namespace TraceFlow.Api.UnitTests.ApiKeys;

public class CreateApiKeyCommandValidatorTests
{
    private readonly CreateApiKeyCommandValidator _validator;

    public CreateApiKeyCommandValidatorTests()
    {
        _validator = new CreateApiKeyCommandValidator();
    }

    [Fact]
    public void Should_be_valid_when_command_is_valid()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_be_invalid_when_name_is_empty(string name)
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            Name = name
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x =>
            x.PropertyName == nameof(CreateApiKeyCommand.Name));
    }

    [Fact]
    public void Should_be_invalid_when_name_exceeds_maximum_length()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            Name = new string('a', 121)
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x =>
            x.PropertyName == nameof(CreateApiKeyCommand.Name));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid")]
    [InlineData("local")]
    [InlineData("development-test")]
    public void Should_be_invalid_when_environment_is_invalid(
        string environment)
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            Environment = environment
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x =>
            x.PropertyName == nameof(CreateApiKeyCommand.Environment));
    }

    [Theory]
    [InlineData("development")]
    [InlineData("staging")]
    [InlineData("production")]
    [InlineData("DEVELOPMENT")]
    [InlineData("Staging")]
    [InlineData(" PRODUCTION ")]
    public void Should_be_valid_when_environment_is_allowed(
        string environment)
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            Environment = environment
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(ApiKeyExpirationPolicies.OneMonth)]
    [InlineData(ApiKeyExpirationPolicies.ThreeMonths)]
    [InlineData(ApiKeyExpirationPolicies.NineMonths)]
    [InlineData(ApiKeyExpirationPolicies.TwelveMonths)]
    public void Should_be_valid_when_expiration_policy_is_allowed(
        int expirationPolicy)
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            ExpirationPolicy = expirationPolicy
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(10)]
    [InlineData(11)]
    [InlineData(13)]
    [InlineData(100)]
    public void Should_be_invalid_when_expiration_policy_is_not_allowed(
        int expirationPolicy)
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            ExpirationPolicy = expirationPolicy
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x =>
            x.PropertyName == nameof(CreateApiKeyCommand.ExpirationPolicy));
    }

    private static CreateApiKeyCommand CreateValidCommand()
    {
        return new CreateApiKeyCommand(
            WorkspaceId: Ulid.NewUlid(),
            ProjectId: Ulid.NewUlid(),
            ApplicationId: Ulid.NewUlid(),
            UserId: Ulid.NewUlid(),
            Name: "Production API Key",
            Environment: ApplicationEnvironments.Production,
            ExpirationPolicy: ApiKeyExpirationPolicies.ThreeMonths);
    }
}
