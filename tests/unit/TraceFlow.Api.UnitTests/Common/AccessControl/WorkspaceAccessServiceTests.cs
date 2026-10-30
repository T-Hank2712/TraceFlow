using FluentAssertions;
using TraceFlow.Api.Application.Common.AccessControl;
using TraceFlow.Api.Application.Common.Exceptions;
using TraceFlow.Api.Domain.Constants;
using TraceFlow.Api.Domain.Entities;


namespace TraceFlow.Api.UnitTests.Common.AccessControl;

public class WorkspaceAccessServiceTests
{
    private readonly WorkspaceAccessService _service;

    public WorkspaceAccessServiceTests()
    {
        _service = new WorkspaceAccessService(null!);
    }

    [Fact]
    public void EnsureWorkspaceIsActive_Should_Not_Throw_When_Workspace_Is_Active()
    {
        // Arrange
        var workspace = CreateWorkspace();

        workspace.Status.Should().Be(ResourceStatuses.Active);

        // Act
        var act = () => _service.EnsureWorkspaceIsActive(
            workspace,
            "Workspace is archived.");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureWorkspaceIsActive_Should_Throw_When_Workspace_Is_Archived()
    {
        // Arrange
        var workspace = CreateWorkspace();
        workspace.Archive();

        workspace.Status.Should().Be(ResourceStatuses.Archived);

        // Act
        var act = () => _service.EnsureWorkspaceIsActive(
            workspace,
            "Workspace is archived.");

        // Assert
        act.Should()
            .Throw<ConflictException>()
            .WithMessage("Workspace is archived.");
    }

    [Theory]
    [InlineData(WorkspaceMemberRoles.Owner)]
    [InlineData(WorkspaceMemberRoles.Admin)]
    public void EnsureWorkspaceManager_Should_Not_Throw_For_Manager_Roles(
        string role)
    {
        // Arrange
        var membership = CreateMembership(role);

        // Act
        var act = () => _service.EnsureWorkspaceManager(
            membership,
            "Manager permission required.");

        // Assert
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(WorkspaceMemberRoles.Member)]
    public void EnsureWorkspaceManager_Should_Throw_For_Non_Manager_Roles(
        string role)
    {
        // Arrange
        var membership = CreateMembership(role);

        // Act
        var act = () => _service.EnsureWorkspaceManager(
            membership,
            "Manager permission required.");

        // Assert
        act.Should()
            .Throw<ForbiddenException>()
            .WithMessage("Manager permission required.");
    }

    [Fact]
    public void EnsureWorkspaceOwner_Should_Not_Throw_For_Owner()
    {
        // Arrange
        var membership = CreateMembership(
            WorkspaceMemberRoles.Owner);

        // Act
        var act = () => _service.EnsureWorkspaceOwner(
            membership,
            "Owner permission required.");

        // Assert
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(WorkspaceMemberRoles.Admin)]
    [InlineData(WorkspaceMemberRoles.Member)]
    public void EnsureWorkspaceOwner_Should_Throw_For_Non_Owner_Roles(
        string role)
    {
        // Arrange
        var membership = CreateMembership(role);

        // Act
        var act = () => _service.EnsureWorkspaceOwner(
            membership,
            "Owner permission required.");

        // Assert
        act.Should()
            .Throw<ForbiddenException>()
            .WithMessage("Owner permission required.");
    }

    [Theory]
    [InlineData(WorkspaceMemberRoles.Owner, true)]
    [InlineData(WorkspaceMemberRoles.Admin, true)]
    [InlineData(WorkspaceMemberRoles.Member, false)]
    public void IsWorkspaceManager_Should_Return_Expected_Result(
        string role,
        bool expected)
    {
        // Arrange
        var membership = CreateMembership(role);

        // Act
        var result = WorkspaceAccessService.IsWorkspaceManager(
            membership);

        // Assert
        result.Should().Be(expected);
    }

    private static Workspace CreateWorkspace()
    {
        return new Workspace(
            Ulid.NewUlid(),
            "Test Workspace",
            "test-workspace",
            "Test workspace description");
    }

    private static WorkspaceMember CreateMembership(string role)
    {
        return new WorkspaceMember(
            Ulid.NewUlid(),
            Ulid.NewUlid(),
            role);
    }

}