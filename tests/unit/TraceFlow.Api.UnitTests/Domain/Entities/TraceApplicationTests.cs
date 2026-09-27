using FluentAssertions;
using TraceFlow.Api.Domain.Constants;
using TraceFlow.Api.Domain.Entities;

namespace TraceFlow.Api.UnitTests.Domain.Entities;

public class TraceApplicationTests
{
    [Fact]
    public void Constructor_Should_Create_Active_Application()
    {
        // Arrange
        var projectId = Ulid.NewUlid();
        var createdByUserId = Ulid.NewUlid();

        // Act
        var application = new TraceApplication(
            projectId,
            createdByUserId,
            "  Test Application  ",
            "  Test-Application  ",
            "  Test description  ");

        // Assert
        application.Id.Should().NotBe(Ulid.Empty);
        application.ProjectId.Should().Be(projectId);
        application.CreatedByUserId.Should().Be(createdByUserId);
        application.Name.Should().Be("Test Application");
        application.Slug.Should().Be("test-application");
        application.Description.Should().Be("Test description");
        application.Status.Should().Be(ResourceStatuses.Active);

        application.CreatedAt.Should().BeCloseTo(
            DateTime.UtcNow,
            TimeSpan.FromSeconds(2));

        application.UpdatedAt.Should().BeCloseTo(
            DateTime.UtcNow,
            TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Constructor_Should_Set_Null_Description_When_Description_Is_Empty()
    {
        // Arrange & Act
        var application = new TraceApplication(
            Ulid.NewUlid(),
            Ulid.NewUlid(),
            "Test Application",
            "test-application",
            "   ");

        // Assert
        application.Description.Should().BeNull();
    }

    [Fact]
    public void Update_Should_Update_Provided_Values()
    {
        // Arrange
        var application = CreateApplication();

        // Act
        application.Update(
            "  Updated Application  ",
            "  Updated-Slug  ",
            "  Updated description  ");

        // Assert
        application.Name.Should().Be("Updated Application");
        application.Slug.Should().Be("updated-slug");
        application.Description.Should().Be("Updated description");
    }

    [Fact]
    public void Update_Should_Keep_Existing_Name_When_Name_Is_Empty()
    {
        // Arrange
        var application = CreateApplication();
        var originalName = application.Name;

        // Act
        application.Update(
            "   ",
            null,
            null);

        // Assert
        application.Name.Should().Be(originalName);
    }

    [Fact]
    public void Update_Should_Keep_Existing_Slug_When_Slug_Is_Empty()
    {
        // Arrange
        var application = CreateApplication();
        var originalSlug = application.Slug;

        // Act
        application.Update(
            null,
            "   ",
            null);

        // Assert
        application.Slug.Should().Be(originalSlug);
    }

    [Fact]
    public void Update_Should_Set_Description_To_Null_When_Description_Is_Empty()
    {
        // Arrange
        var application = CreateApplication();

        // Act
        application.Update(
            null,
            null,
            "   ");

        // Assert
        application.Description.Should().BeNull();
    }

    [Fact]
    public void Update_Should_Normalize_Name_Slug_And_Description()
    {
        // Arrange
        var application = CreateApplication();

        // Act
        application.Update(
            "  UPDATED APP  ",
            "  UPDATED-SLUG  ",
            "  Updated description  ");

        // Assert
        application.Name.Should().Be("UPDATED APP");
        application.Slug.Should().Be("updated-slug");
        application.Description.Should().Be("Updated description");
    }

    [Fact]
    public void Archive_Should_Set_Status_To_Archived()
    {
        // Arrange
        var application = CreateApplication();

        // Act
        application.Archive();

        // Assert
        application.Status.Should().Be(ResourceStatuses.Archived);
    }

    [Fact]
    public void Archive_Should_Update_UpdatedAt_When_Application_Is_Active()
    {
        // Arrange
        var application = CreateApplication();
        var previousUpdatedAt = application.UpdatedAt;

        // Act
        application.Archive();

        // Assert
        application.UpdatedAt.Should().BeOnOrAfter(previousUpdatedAt);
    }

    [Fact]
    public void Archive_Should_Do_Nothing_When_Application_Is_Already_Archived()
    {
        // Arrange
        var application = CreateApplication();
        application.Archive();

        var previousUpdatedAt = application.UpdatedAt;

        // Act
        application.Archive();

        // Assert
        application.Status.Should().Be(ResourceStatuses.Archived);
        application.UpdatedAt.Should().Be(previousUpdatedAt);
    }

    private static TraceApplication CreateApplication()
    {
        return new TraceApplication(
            Ulid.NewUlid(),
            Ulid.NewUlid(),
            "Test Application",
            "test-application",
            "Test application description");
    }
}