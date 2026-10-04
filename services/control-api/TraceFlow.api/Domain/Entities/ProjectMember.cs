namespace TraceFlow.Api.Domain.Entities;

public class ProjectMember : Entity
{
    public Ulid ProjectId { get; private set; }
    public Project Project { get; private set; } = null!;

    public Ulid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public string Role { get; private set; } = ProjectMemberRoles.Viewer;
    public string Status { get; private set; } = MembershipStatuses.Active;

    private ProjectMember() { }

    public ProjectMember(
        Ulid projectId,
        Ulid userId,
        string role,
        DateTimeOffset createdAt)
    {
        Id = Ulid.NewUlid();
        ProjectId = projectId;
        UserId = userId;
        Role = role.Trim().ToLowerInvariant();
        Status = MembershipStatuses.Active;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }
    public void ChangeRole(string role, DateTimeOffset updatedAt)
    {
        Role = role.Trim().ToLowerInvariant();
        UpdatedAt = updatedAt;
    }

    public void Activate(DateTimeOffset actived)
    {
        Status = MembershipStatuses.Active;
        UpdatedAt = actived;
    }
}
