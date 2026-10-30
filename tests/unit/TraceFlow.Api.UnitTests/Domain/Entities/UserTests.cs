using FluentAssertions;
using TraceFlow.Api.Domain.Constants;
using TraceFlow.Api.Domain.Entities;

namespace TraceFlow.Api.UnitTests.Domain.Entities;

public class UserTests
{
    [Fact]
    public void Constructor_Should_Create_User_With_Default_Status_And_Role()
    {
        // Arrange
        const string email = "test@example.com";
        const string userName = "TestUser";
        const string firstName = "Test";
        const string lastName = "User";
        const string passwordHash = "hashed-password";

        // Act
        var user = new User(
            email,
            userName,
            firstName,
            lastName,
            passwordHash);

        // Assert
        user.Id.Should().NotBe(Ulid.Empty);
        user.Email.Should().Be(email);
        user.UserName.Should().Be(userName);
        user.NormalizedUsername.Should().Be("testuser");
        user.FirstName.Should().Be(firstName);
        user.LastName.Should().Be(lastName);
        user.PasswordHash.Should().Be(passwordHash);
        user.Role.Should().Be(UserRoles.User);
        user.Status.Should().Be(UserStatuses.Active);

        user.RefreshTokens.Should().NotBeNull().And.BeEmpty();
        user.WorkspaceMemberships.Should().NotBeNull().And.BeEmpty();
        user.ProjectMemberships.Should().NotBeNull().And.BeEmpty();

        user.CreatedAt.Should().BeCloseTo(
            DateTime.UtcNow,
            TimeSpan.FromSeconds(2));

        user.UpdatedAt.Should().BeCloseTo(
            DateTime.UtcNow,
            TimeSpan.FromSeconds(2));
    }

    [Theory]
    [InlineData("TestUser", "testuser")]
    [InlineData("TESTUSER", "testuser")]
    [InlineData("  TestUser  ", "testuser")]
    [InlineData("John.Doe", "john.doe")]
    [InlineData("  JOHN.DOE  ", "john.doe")]
    public void Constructor_Should_Normalize_Username(
        string userName,
        string expectedNormalizedUsername)
    {
        // Act
        var user = CreateUser(userName: userName);

        // Assert
        user.NormalizedUsername.Should().Be(expectedNormalizedUsername);
    }

    [Fact]
    public void UpdateProfile_Should_Update_Username_And_NormalizedUsername()
    {
        // Arrange
        var user = CreateUser(userName: "olduser");

        // Act
        user.UpdateProfile(
            "  NewUser  ",
            null,
            null);

        // Assert
        user.UserName.Should().Be("NewUser");
        user.NormalizedUsername.Should().Be("newuser");
    }

    [Fact]
    public void UpdateProfile_Should_Update_FirstName_And_LastName()
    {
        // Arrange
        var user = CreateUser(
            firstName: "OldFirst",
            lastName: "OldLast");

        // Act
        user.UpdateProfile(
            null,
            "  NewFirst  ",
            "  NewLast  ");

        // Assert
        user.FirstName.Should().Be("NewFirst");
        user.LastName.Should().Be("NewLast");
    }

    [Fact]
    public void UpdateProfile_Should_Update_All_Provided_Fields()
    {
        // Arrange
        var user = CreateUser(
            userName: "olduser",
            firstName: "OldFirst",
            lastName: "OldLast");

        // Act
        user.UpdateProfile(
            "  NewUser  ",
            "  NewFirst  ",
            "  NewLast  ");

        // Assert
        user.UserName.Should().Be("NewUser");
        user.NormalizedUsername.Should().Be("newuser");
        user.FirstName.Should().Be("NewFirst");
        user.LastName.Should().Be("NewLast");
    }

    [Fact]
    public void UpdateProfile_Should_Keep_Existing_Values_When_Fields_Are_Null()
    {
        // Arrange
        var user = CreateUser(
            userName: "existinguser",
            firstName: "ExistingFirst",
            lastName: "ExistingLast");

        // Act
        user.UpdateProfile(
            null,
            null,
            null);

        // Assert
        user.UserName.Should().Be("existinguser");
        user.NormalizedUsername.Should().Be("existinguser");
        user.FirstName.Should().Be("ExistingFirst");
        user.LastName.Should().Be("ExistingLast");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void UpdateProfile_Should_Ignore_Whitespace_Username(
        string userName)
    {
        // Arrange
        var user = CreateUser(userName: "existinguser");

        // Act
        user.UpdateProfile(
            userName,
            null,
            null);

        // Assert
        user.UserName.Should().Be("existinguser");
        user.NormalizedUsername.Should().Be("existinguser");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void UpdateProfile_Should_Ignore_Whitespace_FirstName(
        string firstName)
    {
        // Arrange
        var user = CreateUser(firstName: "ExistingFirst");

        // Act
        user.UpdateProfile(
            null,
            firstName,
            null);

        // Assert
        user.FirstName.Should().Be("ExistingFirst");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void UpdateProfile_Should_Ignore_Whitespace_LastName(
        string lastName)
    {
        // Arrange
        var user = CreateUser(lastName: "ExistingLast");

        // Act
        user.UpdateProfile(
            null,
            null,
            lastName);

        // Assert
        user.LastName.Should().Be("ExistingLast");
    }

    [Fact]
    public void UpdateProfile_Should_Update_UpdatedAt()
    {
        // Arrange
        var user = CreateUser();
        var previousUpdatedAt = user.UpdatedAt;

        // Act
        user.UpdateProfile(
            "newuser",
            "NewFirst",
            "NewLast");

        // Assert
        user.UpdatedAt.Should().BeOnOrAfter(previousUpdatedAt);
    }

    [Fact]
    public void ChangePassword_Should_Update_PasswordHash()
    {
        // Arrange
        var user = CreateUser(
            passwordHash: "old-password-hash");

        // Act
        user.ChangePassword("new-password-hash");

        // Assert
        user.PasswordHash.Should().Be("new-password-hash");
    }

    [Fact]
    public void ChangePassword_Should_Update_UpdatedAt()
    {
        // Arrange
        var user = CreateUser();
        var previousUpdatedAt = user.UpdatedAt;

        // Act
        user.ChangePassword("new-password-hash");

        // Assert
        user.UpdatedAt.Should().BeOnOrAfter(previousUpdatedAt);
    }

    private static User CreateUser(
        string userName = "TestUser",
        string firstName = "Test",
        string lastName = "User",
        string passwordHash = "hashed-password")
    {
        return new User(
            "test@example.com",
            userName,
            firstName,
            lastName,
            passwordHash);
    }
}