using FluentAssertions;
using TraceFlow.Api.Domain.Constants;
using TraceFlow.Api.Domain.Entities;

namespace TraceFlow.Api.UnitTests.Domain.Entities;

public class ProjectMemberTests
{
    [Fact]
    public void Constructor_Should_Create_Active_Member()
    {
        // Arrange
        var projectId = Ulid.NewUlid();
        var userId = Ulid.NewUlid();

        // Act
        var member = new ProjectMember(
            projectId,
            userId,
            "  MANAGER  ");

        // Assert
        member.Id.Should().NotBe(Ulid.Empty);
        member.ProjectId.Should().Be(projectId);
        member.UserId.Should().Be(userId);
        member.Role.Should().Be(ProjectMemberRoles.Manager);
        member.Status.Should().Be(MembershipStatuses.Active);

        member.JoinedAt.Should().BeCloseTo(
            DateTime.UtcNow,
            TimeSpan.FromSeconds(2));

        member.CreatedAt.Should().BeCloseTo(
            DateTime.UtcNow,
            TimeSpan.FromSeconds(2));

        member.UpdatedAt.Should().BeCloseTo(
            DateTime.UtcNow,
            TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Constructor_Should_Normalize_Role()
    {
        // Arrange
        var projectId = Ulid.NewUlid();
        var userId = Ulid.NewUlid();

        // Act
        var member = new ProjectMember(
            projectId,
            userId,
            "  DeVeLoPeR  ");

        // Assert
        member.Role.Should().Be("developer");
    }

    [Fact]
    public void ChangeRole_Should_Update_Role()
    {
        // Arrange
        var member = CreateMember();

        // Act
        member.ChangeRole("  MANAGER  ");

        // Assert
        member.Role.Should().Be(ProjectMemberRoles.Manager);
    }

    [Fact]
    public void ChangeRole_Should_Normalize_Role()
    {
        // Arrange
        var member = CreateMember();

        // Act
        member.ChangeRole("  DeVeLoPeR  ");

        // Assert
        member.Role.Should().Be("developer");
    }

    [Fact]
    public void ChangeRole_Should_Update_UpdatedAt()
    {
        // Arrange
        var member = CreateMember();
        var previousUpdatedAt = member.UpdatedAt;

        // Act
        member.ChangeRole(ProjectMemberRoles.Manager);

        // Assert
        member.UpdatedAt.Should().BeOnOrAfter(previousUpdatedAt);
    }

    [Fact]
    public void Activate_Should_Set_Status_To_Active()
    {
        // Arrange
        var member = CreateMember();

        // Act
        member.Activate();

        // Assert
        member.Status.Should().Be(MembershipStatuses.Active);
    }

    [Fact]
    public void Activate_Should_Update_UpdatedAt()
    {
        // Arrange
        var member = CreateMember();
        var previousUpdatedAt = member.UpdatedAt;

        // Act
        member.Activate();

        // Assert
        member.UpdatedAt.Should().BeOnOrAfter(previousUpdatedAt);
    }

    private static ProjectMember CreateMember()
    {
        return new ProjectMember(
            Ulid.NewUlid(),
            Ulid.NewUlid(),
            ProjectMemberRoles.Viewer);
    }
}