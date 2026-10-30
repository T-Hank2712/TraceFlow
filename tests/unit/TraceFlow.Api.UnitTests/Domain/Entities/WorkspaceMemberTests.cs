using FluentAssertions;
using TraceFlow.Api.Domain.Constants;
using TraceFlow.Api.Domain.Entities;

namespace TraceFlow.Api.UnitTests.Domain.Entities;

public class WorkspaceMemberTests
{
    [Fact]
    public void Constructor_Should_Create_Active_Member()
    {
        // Arrange
        var workspaceId = Ulid.NewUlid();
        var userId = Ulid.NewUlid();

        // Act
        var member = new WorkspaceMember(
            workspaceId,
            userId,
            "  ADMIN  ");

        // Assert
        member.Id.Should().NotBe(Ulid.Empty);
        member.WorkspaceId.Should().Be(workspaceId);
        member.UserId.Should().Be(userId);
        member.Role.Should().Be(WorkspaceMemberRoles.Admin);
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
        var workspaceId = Ulid.NewUlid();
        var userId = Ulid.NewUlid();

        // Act
        var member = new WorkspaceMember(
            workspaceId,
            userId,
            "  MaNaGeR  ");

        // Assert
        member.Role.Should().Be("manager");
    }

    [Fact]
    public void ChangeRole_Should_Update_Role()
    {
        // Arrange
        var member = CreateMember();

        // Act
        member.ChangeRole("  ADMIN  ");

        // Assert
        member.Role.Should().Be(WorkspaceMemberRoles.Admin);
    }

    [Fact]
    public void ChangeRole_Should_Normalize_Role()
    {
        // Arrange
        var member = CreateMember();

        // Act
        member.ChangeRole("  MaNaGeR  ");

        // Assert
        member.Role.Should().Be("manager");
    }

    [Fact]
    public void ChangeRole_Should_Update_UpdatedAt()
    {
        // Arrange
        var member = CreateMember();
        var previousUpdatedAt = member.UpdatedAt;

        // Act
        member.ChangeRole(WorkspaceMemberRoles.Admin);

        // Assert
        member.UpdatedAt.Should().BeOnOrAfter(previousUpdatedAt);
    }

    [Fact]
    public void Remove_Should_Set_Status_To_Removed()
    {
        // Arrange
        var member = CreateMember();

        // Act
        member.Remove();

        // Assert
        member.Status.Should().Be(MembershipStatuses.Removed);
    }

    [Fact]
    public void Remove_Should_Update_UpdatedAt()
    {
        // Arrange
        var member = CreateMember();
        var previousUpdatedAt = member.UpdatedAt;

        // Act
        member.Remove();

        // Assert
        member.UpdatedAt.Should().BeOnOrAfter(previousUpdatedAt);
    }

    [Fact]
    public void Activate_Should_Set_Status_To_Active()
    {
        // Arrange
        var member = CreateMember();
        member.Remove();

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
        member.Remove();

        var previousUpdatedAt = member.UpdatedAt;

        // Act
        member.Activate();

        // Assert
        member.UpdatedAt.Should().BeOnOrAfter(previousUpdatedAt);
    }

    private static WorkspaceMember CreateMember()
    {
        return new WorkspaceMember(
            Ulid.NewUlid(),
            Ulid.NewUlid(),
            WorkspaceMemberRoles.Member);
    }
}