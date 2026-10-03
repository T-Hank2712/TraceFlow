namespace TraceFlow.Api.Domain.Entities;

public class WorkspaceMember : Entity
{
    public Ulid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public Ulid WorkspaceId { get; private set; }
    public Workspace Workspace { get; private set; } = null!;
    public string Role { get; private set; } = WorkspaceMemberRoles.Member;
    public string Status { get; private set; } = MembershipStatuses.Active;
    private WorkspaceMember() { }
    public WorkspaceMember(
        Ulid workspaceId,
        Ulid userId,
        string role,
        DateTimeOffset createdAt)
    {
        Id = Ulid.NewUlid();
        WorkspaceId = workspaceId;
        UserId = userId;
        Role = role.Trim().ToLower();
        Status = MembershipStatuses.Active;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }
    public void ChangeRole(string role, DateTimeOffset updatedAt)
    {
        Role = role.Trim().ToLowerInvariant();
        UpdatedAt = updatedAt;
    }
    public void Remove(DateTimeOffset removedAt)
    {
        Status = MembershipStatuses.Removed;
        UpdatedAt = removedAt;
    }
    public void Activate(DateTimeOffset activatedAt)
    {
        Status = MembershipStatuses.Active;
        UpdatedAt = activatedAt;
    }
}
