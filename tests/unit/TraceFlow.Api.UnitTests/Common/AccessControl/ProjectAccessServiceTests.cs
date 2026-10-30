using FluentAssertions;
using TraceFlow.Api.Application.Common.AccessControl;
using TraceFlow.Api.Application.Common.Exceptions;
using TraceFlow.Api.Domain.Constants;
using TraceFlow.Api.Domain.Entities;

namespace TraceFlow.Api.UnitTests.Common.AccessControl;

public class ProjectAccessServiceTests
{
    private readonly ProjectAccessService _service;

    public ProjectAccessServiceTests()
    {
        _service = new ProjectAccessService(
            dbContext: null!,
            workspaceAccessService: null!);
    }

    #region ProjectAccessContext

    [Theory]
    [InlineData(WorkspaceMemberRoles.Owner, true)]
    [InlineData(WorkspaceMemberRoles.Admin, true)]
    [InlineData(WorkspaceMemberRoles.Member, false)]
    public void IsWorkspaceManager_Should_Return_Expected_Result(
        string workspaceRole,
        bool expected)
    {
        // Arrange
        var workspaceMember = CreateWorkspaceMember(workspaceRole);
        var project = CreateProject();

        var access = new ProjectAccessContext(
            workspaceMember,
            project,
            ProjectMembership: null);

        // Act
        var result = access.IsWorkspaceManager;

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(ProjectMemberRoles.Manager, true)]
    [InlineData(ProjectMemberRoles.Developer, false)]
    [InlineData(ProjectMemberRoles.Viewer, false)]
    public void IsProjectManager_Should_Return_Expected_Result(
        string projectRole,
        bool expected)
    {
        // Arrange
        var workspaceMember = CreateWorkspaceMember(
            WorkspaceMemberRoles.Member);

        var project = CreateProject();

        var projectMember = CreateProjectMember(projectRole);

        var access = new ProjectAccessContext(
            workspaceMember,
            project,
            projectMember);

        // Act
        var result = access.IsProjectManager;

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void IsProjectManager_Should_Return_False_When_ProjectMembership_Is_Null()
    {
        // Arrange
        var workspaceMember = CreateWorkspaceMember(
            WorkspaceMemberRoles.Member);

        var project = CreateProject();

        var access = new ProjectAccessContext(
            workspaceMember,
            project,
            ProjectMembership: null);

        // Act
        var result = access.IsProjectManager;

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region EnsureProjectManager

    [Theory]
    [InlineData(WorkspaceMemberRoles.Owner, null)]
    [InlineData(WorkspaceMemberRoles.Admin, null)]
    [InlineData(
        WorkspaceMemberRoles.Member,
        ProjectMemberRoles.Manager)]
    public void EnsureProjectManager_Should_Not_Throw_When_User_Is_Manager(
        string workspaceRole,
        string? projectRole)
    {
        // Arrange
        var access = CreateAccessContext(
            workspaceRole,
            projectRole);

        // Act
        var act = () => _service.EnsureProjectManager(
            access,
            "User is not allowed to manage this project.");

        // Assert
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(WorkspaceMemberRoles.Member, ProjectMemberRoles.Developer)]
    [InlineData(WorkspaceMemberRoles.Member, ProjectMemberRoles.Viewer)]
    [InlineData(WorkspaceMemberRoles.Member, null)]
    public void EnsureProjectManager_Should_Throw_When_User_Is_Not_Manager(
        string workspaceRole,
        string? projectRole)
    {
        // Arrange
        var access = CreateAccessContext(
            workspaceRole,
            projectRole);

        // Act
        var act = () => _service.EnsureProjectManager(
            access,
            "User is not allowed to manage this project.");

        // Assert
        act.Should()
            .Throw<ForbiddenException>()
            .WithMessage("User is not allowed to manage this project.");
    }

    #endregion

    #region EnsureProjectDeveloperOrManager

    [Theory]
    [InlineData(WorkspaceMemberRoles.Owner, null)]
    [InlineData(WorkspaceMemberRoles.Admin, null)]
    [InlineData(
        WorkspaceMemberRoles.Member,
        ProjectMemberRoles.Manager)]
    [InlineData(
        WorkspaceMemberRoles.Member,
        ProjectMemberRoles.Developer)]
    public void EnsureProjectDeveloperOrManager_Should_Not_Throw_When_User_Has_Access(
        string workspaceRole,
        string? projectRole)
    {
        // Arrange
        var access = CreateAccessContext(
            workspaceRole,
            projectRole);

        // Act
        var act = () => _service.EnsureProjectDeveloperOrManager(
            access,
            "User is not allowed to access this project.");

        // Assert
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(WorkspaceMemberRoles.Member, ProjectMemberRoles.Viewer)]
    [InlineData(WorkspaceMemberRoles.Member, null)]
    public void EnsureProjectDeveloperOrManager_Should_Throw_When_User_Lacks_Access(
        string workspaceRole,
        string? projectRole)
    {
        // Arrange
        var access = CreateAccessContext(
            workspaceRole,
            projectRole);

        // Act
        var act = () => _service.EnsureProjectDeveloperOrManager(
            access,
            "User is not allowed to access this project.");

        // Assert
        act.Should()
            .Throw<ForbiddenException>()
            .WithMessage("User is not allowed to access this project.");
    }

    #endregion

    #region EnsureProjectMember

    [Theory]
    [InlineData(WorkspaceMemberRoles.Owner)]
    [InlineData(WorkspaceMemberRoles.Admin)]
    public void EnsureProjectMember_Should_Not_Throw_When_User_Is_Workspace_Manager(
        string workspaceRole)
    {
        // Arrange
        var access = CreateAccessContext(
            workspaceRole,
            projectRole: null);

        // Act
        var act = () => _service.EnsureProjectMember(
            access,
            "User is not a project member.");

        // Assert
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(ProjectMemberRoles.Manager)]
    [InlineData(ProjectMemberRoles.Developer)]
    [InlineData(ProjectMemberRoles.Viewer)]
    public void EnsureProjectMember_Should_Not_Throw_When_User_Is_Project_Member(
        string projectRole)
    {
        // Arrange
        var access = CreateAccessContext(
            WorkspaceMemberRoles.Member,
            projectRole);

        // Act
        var act = () => _service.EnsureProjectMember(
            access,
            "User is not a project member.");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureProjectMember_Should_Throw_When_User_Has_No_Project_Membership()
    {
        // Arrange
        var access = CreateAccessContext(
            WorkspaceMemberRoles.Member,
            projectRole: null);

        // Act
        var act = () => _service.EnsureProjectMember(
            access,
            "User is not a project member.");

        // Assert
        act.Should()
            .Throw<ForbiddenException>()
            .WithMessage("User is not a project member.");
    }

    #endregion

    #region Helpers

    private static ProjectAccessContext CreateAccessContext(
        string workspaceRole,
        string? projectRole)
    {
        var workspaceMember = CreateWorkspaceMember(workspaceRole);
        var project = CreateProject();

        ProjectMember? projectMember = projectRole is null
            ? null
            : CreateProjectMember(projectRole);

        return new ProjectAccessContext(
            workspaceMember,
            project,
            projectMember);
    }

    private static WorkspaceMember CreateWorkspaceMember(string role)
    {
        return new WorkspaceMember(
            Ulid.NewUlid(),
            Ulid.NewUlid(),
            role);
    }

    private static ProjectMember CreateProjectMember(string role)
    {
        return new ProjectMember(
            Ulid.NewUlid(),
            Ulid.NewUlid(),
            role);
    }

    private static Project CreateProject()
    {
        return new Project(
            Ulid.NewUlid(), // Project Id
            Ulid.NewUlid(), // Workspace Id
            "Test Project",
            "test-project",
            "Test project description");
    }

    #endregion
}