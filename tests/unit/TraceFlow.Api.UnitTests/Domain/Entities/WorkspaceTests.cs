using FluentAssertions;
using TraceFlow.Api.Domain.Constants;
using TraceFlow.Api.Domain.Entities;

namespace TraceFlow.Api.UnitTests.Domain.Entities;

public class WorkspaceTests
{
    [Fact]
    public void Constructor_Should_Create_Active_Workspace()
    {
        // Arrange
        var ownerUserId = Ulid.NewUlid();

        // Act
        var workspace = new Workspace(
            ownerUserId,
            "  Test Workspace  ",
            "  Test-Workspace  ",
            "  Test description  ");

        // Assert
        workspace.Id.Should().NotBe(Ulid.Empty);
        workspace.OwnerUserId.Should().Be(ownerUserId);
        workspace.Name.Should().Be("Test Workspace");
        workspace.Slug.Should().Be("test-workspace");
        workspace.Description.Should().Be("Test description");
        workspace.Status.Should().Be(ResourceStatuses.Active);
        workspace.CreatedAt.Should().BeCloseTo(
            DateTime.UtcNow,
            TimeSpan.FromSeconds(2));
        workspace.UpdatedAt.Should().BeCloseTo(
            DateTime.UtcNow,
            TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Constructor_Should_Set_Null_Description_When_Description_Is_Empty()
    {
        // Arrange & Act
        var workspace = new Workspace(
            Ulid.NewUlid(),
            "Test Workspace",
            "test-workspace",
            "   ");

        // Assert
        workspace.Description.Should().BeNull();
    }

    [Fact]
    public void UpdateWorkspace_Should_Update_Provided_Values()
    {
        // Arrange
        var workspace = CreateWorkspace();

        // Act
        workspace.UpdateWorkspace(
            "  Updated Workspace  ",
            "  Updated-Slug  ",
            "  Updated description  ");

        // Assert
        workspace.Name.Should().Be("Updated Workspace");
        workspace.Slug.Should().Be("updated-slug");
        workspace.Description.Should().Be("Updated description");
    }

    [Fact]
    public void UpdateWorkspace_Should_Keep_Existing_Name_When_Name_Is_Empty()
    {
        // Arrange
        var workspace = CreateWorkspace();
        var originalName = workspace.Name;

        // Act
        workspace.UpdateWorkspace(
            "   ",
            null,
            null);

        // Assert
        workspace.Name.Should().Be(originalName);
    }

    [Fact]
    public void UpdateWorkspace_Should_Keep_Existing_Slug_When_Slug_Is_Empty()
    {
        // Arrange
        var workspace = CreateWorkspace();
        var originalSlug = workspace.Slug;

        // Act
        workspace.UpdateWorkspace(
            null,
            "   ",
            null);

        // Assert
        workspace.Slug.Should().Be(originalSlug);
    }

    [Fact]
    public void UpdateWorkspace_Should_Set_Description_To_Null_When_Description_Is_Empty()
    {
        // Arrange
        var workspace = CreateWorkspace();

        // Act
        workspace.UpdateWorkspace(
            null,
            null,
            "   ");

        // Assert
        workspace.Description.Should().BeNull();
    }

    [Fact]
    public void Archive_Should_Set_Status_To_Archived()
    {
        // Arrange
        var workspace = CreateWorkspace();

        // Act
        workspace.Archive();

        // Assert
        workspace.Status.Should().Be(ResourceStatuses.Archived);
    }

    [Fact]
    public void Activate_Should_Set_Status_To_Active()
    {
        // Arrange
        var workspace = CreateWorkspace();
        workspace.Archive();

        // Act
        workspace.Activate();

        // Assert
        workspace.Status.Should().Be(ResourceStatuses.Active);
    }

    [Fact]
    public void Archive_Should_Update_UpdatedAt()
    {
        // Arrange
        var workspace = CreateWorkspace();
        var previousUpdatedAt = workspace.UpdatedAt;

        // Act
        workspace.Archive();

        // Assert
        workspace.UpdatedAt.Should().BeOnOrAfter(previousUpdatedAt);
    }

    [Fact]
    public void Activate_Should_Update_UpdatedAt()
    {
        // Arrange
        var workspace = CreateWorkspace();
        workspace.Archive();

        var previousUpdatedAt = workspace.UpdatedAt;

        // Act
        workspace.Activate();

        // Assert
        workspace.UpdatedAt.Should().BeOnOrAfter(previousUpdatedAt);
    }

    private static Workspace CreateWorkspace()
    {
        return new Workspace(
            Ulid.NewUlid(),
            "Test Workspace",
            "test-workspace",
            "Test workspace description");
    }
}