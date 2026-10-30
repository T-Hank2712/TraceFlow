using FluentAssertions;
using TraceFlow.Api.Domain.Constants;
using TraceFlow.Api.Domain.Entities;

namespace TraceFlow.Api.UnitTests.Domain.Entities;

public class ProjectInvitationTests
{
    [Fact]
    public void Constructor_Should_Create_Pending_Invitation()
    {
        // Arrange
        var workspaceId = Ulid.NewUlid();
        var projectId = Ulid.NewUlid();
        var invitedUserId = Ulid.NewUlid();
        var invitedByUserId = Ulid.NewUlid();
        var expiresAt = DateTime.UtcNow.AddDays(7);

        // Act
        var invitation = new ProjectInvitation(
            workspaceId,
            projectId,
            invitedUserId,
            invitedByUserId,
            "  MANAGER  ",
            expiresAt);

        // Assert
        invitation.Id.Should().NotBe(Ulid.Empty);
        invitation.WorkspaceId.Should().Be(workspaceId);
        invitation.ProjectId.Should().Be(projectId);
        invitation.InvitedUserId.Should().Be(invitedUserId);
        invitation.InvitedByUserId.Should().Be(invitedByUserId);
        invitation.Role.Should().Be(ProjectMemberRoles.Manager);
        invitation.Status.Should().Be(InvitationStatuses.Pending);
        invitation.ExpiresAt.Should().Be(expiresAt);

        invitation.CreatedAt.Should().BeCloseTo(
            DateTime.UtcNow,
            TimeSpan.FromSeconds(2));

        invitation.UpdatedAt.Should().BeCloseTo(
            DateTime.UtcNow,
            TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Constructor_Should_Normalize_Role()
    {
        // Arrange & Act
        var invitation = CreateInvitation(
            expiresAt: DateTime.UtcNow.AddDays(7));

        // Assert
        invitation.Role.Should().Be(ProjectMemberRoles.Viewer);
    }

    [Fact]
    public void Accept_Should_Set_Status_To_Accepted()
    {
        // Arrange
        var invitation = CreateInvitation(
            expiresAt: DateTime.UtcNow.AddDays(7));

        // Act
        invitation.Accept();

        // Assert
        invitation.Status.Should().Be(InvitationStatuses.Accepted);
    }

    [Fact]
    public void Accept_Should_Update_UpdatedAt()
    {
        // Arrange
        var invitation = CreateInvitation(
            expiresAt: DateTime.UtcNow.AddDays(7));

        var previousUpdatedAt = invitation.UpdatedAt;

        // Act
        invitation.Accept();

        // Assert
        invitation.UpdatedAt.Should().BeOnOrAfter(previousUpdatedAt);
    }

    [Fact]
    public void Accept_Should_Throw_When_Invitation_Is_Not_Pending()
    {
        // Arrange
        var invitation = CreateInvitation(
            expiresAt: DateTime.UtcNow.AddDays(7));

        invitation.Cancel();

        // Act
        var act = () => invitation.Accept();

        // Assert
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Invitation is not pending.");
    }

    [Fact]
    public void Accept_Should_Throw_When_Invitation_Is_Expired()
    {
        // Arrange
        var invitation = CreateInvitation(
            expiresAt: DateTime.UtcNow.AddMinutes(-1));

        // Act
        var act = () => invitation.Accept();

        // Assert
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Invitation has expired.");
    }

    [Fact]
    public void Decline_Should_Set_Status_To_Declined()
    {
        // Arrange
        var invitation = CreateInvitation(
            expiresAt: DateTime.UtcNow.AddDays(7));

        // Act
        invitation.Decline();

        // Assert
        invitation.Status.Should().Be(InvitationStatuses.Declined);
    }

    [Fact]
    public void Decline_Should_Update_UpdatedAt()
    {
        // Arrange
        var invitation = CreateInvitation(
            expiresAt: DateTime.UtcNow.AddDays(7));

        var previousUpdatedAt = invitation.UpdatedAt;

        // Act
        invitation.Decline();

        // Assert
        invitation.UpdatedAt.Should().BeOnOrAfter(previousUpdatedAt);
    }

    [Fact]
    public void Decline_Should_Throw_When_Invitation_Is_Not_Pending()
    {
        // Arrange
        var invitation = CreateInvitation(
            expiresAt: DateTime.UtcNow.AddDays(7));

        invitation.Cancel();

        // Act
        var act = () => invitation.Decline();

        // Assert
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Invitation is not pending.");
    }

    [Fact]
    public void Decline_Should_Throw_When_Invitation_Is_Expired()
    {
        // Arrange
        var invitation = CreateInvitation(
            expiresAt: DateTime.UtcNow.AddMinutes(-1));

        // Act
        var act = () => invitation.Decline();

        // Assert
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Invitation has expired.");
    }

    [Fact]
    public void Cancel_Should_Set_Status_To_Cancelled()
    {
        // Arrange
        var invitation = CreateInvitation(
            expiresAt: DateTime.UtcNow.AddDays(7));

        // Act
        invitation.Cancel();

        // Assert
        invitation.Status.Should().Be(InvitationStatuses.Cancelled);
    }

    [Fact]
    public void Cancel_Should_Update_UpdatedAt()
    {
        // Arrange
        var invitation = CreateInvitation(
            expiresAt: DateTime.UtcNow.AddDays(7));

        var previousUpdatedAt = invitation.UpdatedAt;

        // Act
        invitation.Cancel();

        // Assert
        invitation.UpdatedAt.Should().BeOnOrAfter(previousUpdatedAt);
    }

    [Fact]
    public void Cancel_Should_Not_Require_Invitation_To_Be_Unexpired()
    {
        // Arrange
        var invitation = CreateInvitation(
            expiresAt: DateTime.UtcNow.AddMinutes(-1));

        // Act
        invitation.Cancel();

        // Assert
        invitation.Status.Should().Be(InvitationStatuses.Cancelled);
    }

    [Fact]
    public void Cancel_Should_Throw_When_Invitation_Is_Not_Pending()
    {
        // Arrange
        var invitation = CreateInvitation(
            expiresAt: DateTime.UtcNow.AddDays(7));

        invitation.Accept();

        // Act
        var act = () => invitation.Cancel();

        // Assert
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Only pending invitation can be cancelled.");
    }

    private static ProjectInvitation CreateInvitation(
        DateTime expiresAt)
    {
        return new ProjectInvitation(
            Ulid.NewUlid(),
            Ulid.NewUlid(),
            Ulid.NewUlid(),
            Ulid.NewUlid(),
            ProjectMemberRoles.Viewer,
            expiresAt);
    }
}