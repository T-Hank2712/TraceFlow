using FluentAssertions;
using TraceFlow.Api.Domain.Constants;
using TraceFlow.Api.Domain.Entities;

namespace TraceFlow.Api.UnitTests.Domain.Entities;

public class ProjectTests
{
    [Fact]
    public void Constructor_Should_Create_Active_Project()
    {
        // Arrange
        var workspaceId = Ulid.NewUlid();
        var createdByUserId = Ulid.NewUlid();

        // Act
        var project = new Project(
            workspaceId,
            createdByUserId,
            "  Test Project  ",
            "  Test-Project  ",
            "  Test description  ");

        // Assert
        project.Id.Should().NotBe(Ulid.Empty);
        project.WorkspaceId.Should().Be(workspaceId);
        project.CreatedByUserId.Should().Be(createdByUserId);
        project.Name.Should().Be("Test Project");
        project.Slug.Should().Be("test-project");
        project.Description.Should().Be("Test description");
        project.Status.Should().Be(ResourceStatuses.Active);
        project.CreatedAt.Should().BeCloseTo(
            DateTime.UtcNow,
            TimeSpan.FromSeconds(2));
        project.UpdatedAt.Should().BeCloseTo(
            DateTime.UtcNow,
            TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Constructor_Should_Set_Null_Description_When_Description_Is_Empty()
    {
        // Arrange & Act
        var project = new Project(
            Ulid.NewUlid(),
            Ulid.NewUlid(),
            "Test Project",
            "test-project",
            "   ");

        // Assert
        project.Description.Should().BeNull();
    }

    [Fact]
    public void UpdateProject_Should_Update_Provided_Values()
    {
        // Arrange
        var project = CreateProject();

        // Act
        project.UpdateProject(
            "  Updated Project  ",
            "  Updated-Slug  ",
            "  Updated description  ");

        // Assert
        project.Name.Should().Be("Updated Project");
        project.Slug.Should().Be("updated-slug");
        project.Description.Should().Be("Updated description");
    }

    [Fact]
    public void UpdateProject_Should_Keep_Existing_Name_When_Name_Is_Empty()
    {
        // Arrange
        var project = CreateProject();
        var originalName = project.Name;

        // Act
        project.UpdateProject(
            "   ",
            null,
            null);

        // Assert
        project.Name.Should().Be(originalName);
    }

    [Fact]
    public void UpdateProject_Should_Keep_Existing_Slug_When_Slug_Is_Empty()
    {
        // Arrange
        var project = CreateProject();
        var originalSlug = project.Slug;

        // Act
        project.UpdateProject(
            null,
            "   ",
            null);

        // Assert
        project.Slug.Should().Be(originalSlug);
    }

    [Fact]
    public void UpdateProject_Should_Set_Description_To_Null_When_Description_Is_Empty()
    {
        // Arrange
        var project = CreateProject();

        // Act
        project.UpdateProject(
            null,
            null,
            "   ");

        // Assert
        project.Description.Should().BeNull();
    }

    [Fact]
    public void Archive_Should_Set_Status_To_Archived()
    {
        // Arrange
        var project = CreateProject();

        // Act
        project.Archive();

        // Assert
        project.Status.Should().Be(ResourceStatuses.Archived);
    }

    [Fact]
    public void Activate_Should_Set_Status_To_Active()
    {
        // Arrange
        var project = CreateProject();
        project.Archive();

        // Act
        project.Activate();

        // Assert
        project.Status.Should().Be(ResourceStatuses.Active);
    }

    [Fact]
    public void Archive_Should_Update_UpdatedAt()
    {
        // Arrange
        var project = CreateProject();
        var previousUpdatedAt = project.UpdatedAt;

        // Act
        project.Archive();

        // Assert
        project.UpdatedAt.Should().BeOnOrAfter(previousUpdatedAt);
    }

    [Fact]
    public void Activate_Should_Update_UpdatedAt()
    {
        // Arrange
        var project = CreateProject();
        project.Archive();

        var previousUpdatedAt = project.UpdatedAt;

        // Act
        project.Activate();

        // Assert
        project.UpdatedAt.Should().BeOnOrAfter(previousUpdatedAt);
    }

    private static Project CreateProject()
    {
        return new Project(
            Ulid.NewUlid(), // ProjectId
            Ulid.NewUlid(), // WorkspaceId
            "Test Project",
            "test-project",
            "Test project description");
    }
}