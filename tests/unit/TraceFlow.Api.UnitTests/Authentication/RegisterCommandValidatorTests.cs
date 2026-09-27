using FluentAssertions;
using TraceFlow.Api.Application.Auth.Commands.Register;
using Xunit;

namespace TraceFlow.Api.UnitTests.Authentication;

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator;

    public RegisterCommandValidatorTests()
    {
        _validator = new RegisterCommandValidator();
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
    [InlineData("invalid-email")]
    [InlineData("invalid@")]
    [InlineData("@example.com")]
    public void Should_be_invalid_when_email_is_invalid(string email)
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            Email = email
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x =>
            x.PropertyName == nameof(RegisterCommand.Email));
    }

    [Fact]
    public void Should_be_invalid_when_email_exceeds_maximum_length()
    {
        // Arrange
        var email = $"{new string('a', 92)}@test.com";

        var command = CreateValidCommand() with
        {
            Email = email
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x =>
            x.PropertyName == nameof(RegisterCommand.Email));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_be_invalid_when_username_is_empty(string username)
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            UserName = username
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x =>
            x.PropertyName == nameof(RegisterCommand.UserName));
    }

    [Fact]
    public void Should_be_invalid_when_username_exceeds_maximum_length()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            UserName = new string('a', 51)
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x =>
            x.PropertyName == nameof(RegisterCommand.UserName));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_be_invalid_when_first_name_is_empty(string firstName)
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            FirstName = firstName
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x =>
            x.PropertyName == nameof(RegisterCommand.FirstName));
    }

    [Fact]
    public void Should_be_invalid_when_first_name_exceeds_maximum_length()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            FirstName = new string('a', 101)
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x =>
            x.PropertyName == nameof(RegisterCommand.FirstName));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_be_invalid_when_last_name_is_empty(string lastName)
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            LastName = lastName
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x =>
            x.PropertyName == nameof(RegisterCommand.LastName));
    }

    [Fact]
    public void Should_be_invalid_when_last_name_exceeds_maximum_length()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            LastName = new string('a', 101)
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x =>
            x.PropertyName == nameof(RegisterCommand.LastName));
    }

    [Theory]
    [InlineData("Ab1!")]
    [InlineData("A1!")]
    [InlineData("Ab!")]
    public void Should_be_invalid_when_password_is_too_short(string password)
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            Password = password,
            ConfirmPassword = password
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x =>
            x.PropertyName == nameof(RegisterCommand.Password));
    }

    [Fact]
    public void Should_be_invalid_when_password_exceeds_maximum_length()
    {
        // Arrange
        var password = new string('a', 98) + "A1!";

        var command = CreateValidCommand() with
        {
            Password = password,
            ConfirmPassword = password
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x =>
            x.PropertyName == nameof(RegisterCommand.Password));
    }

    [Theory]
    [InlineData("password123!")]
    [InlineData("password!123")]
    public void Should_be_invalid_when_password_has_no_uppercase_letter(
        string password)
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            Password = password,
            ConfirmPassword = password
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x =>
            x.PropertyName == nameof(RegisterCommand.Password));
    }

    [Theory]
    [InlineData("PASSWORD123!")]
    [InlineData("PASSWORD!123")]
    public void Should_be_invalid_when_password_has_no_lowercase_letter(
        string password)
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            Password = password,
            ConfirmPassword = password
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x =>
            x.PropertyName == nameof(RegisterCommand.Password));
    }

    [Theory]
    [InlineData("Password!")]
    [InlineData("Password!!")]
    public void Should_be_invalid_when_password_has_no_number(
        string password)
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            Password = password,
            ConfirmPassword = password
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x =>
            x.PropertyName == nameof(RegisterCommand.Password));
    }

    [Theory]
    [InlineData("Password123")]
    [InlineData("Password1234")]
    public void Should_be_invalid_when_password_has_no_special_character(
        string password)
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            Password = password,
            ConfirmPassword = password
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x =>
            x.PropertyName == nameof(RegisterCommand.Password));
    }

    [Theory]
    [InlineData("")]
    [InlineData("DifferentPassword123!")]
    public void Should_be_invalid_when_confirm_password_is_invalid(
        string confirmPassword)
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            ConfirmPassword = confirmPassword
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x =>
            x.PropertyName == nameof(RegisterCommand.ConfirmPassword));
    }

    private static RegisterCommand CreateValidCommand()
    {
        return new RegisterCommand(
            Email: "john.doe@example.com",
            UserName: "john.doe",
            FirstName: "John",
            LastName: "Doe",
            Password: "Password123!",
            ConfirmPassword: "Password123!"
        );
    }
}