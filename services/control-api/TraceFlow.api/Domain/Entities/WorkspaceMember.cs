using TraceFlow.Api.Domain.Common;
using TraceFlow.Api.Domain.Constants;

namespace TraceFlow.Api.Domain.Entities;
public class WorkspaceMember : Entity
{
    public Ulid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public Ulid WorkspaceId { get; private set; }
    public Workspace Workspace { get; private set; } = null!;
    public string Role { get; private set; } = WorkspaceMemberRoles.Member;
    public string Status { get; private set; } = MembershipStatuses.Active;
    public DateTime JoinedAt { get; private set; }
    private WorkspaceMember() {}
    public WorkspaceMember(Ulid WorkspaceId, Ulid UserId, string Role)
    {
        Id = Ulid.NewUlid();
        this.WorkspaceId = WorkspaceId;
        this.UserId = UserId;
        this.Role = Role.Trim().ToLower();
        Status = MembershipStatuses.Active;
        JoinedAt = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
    public void ChangeRole(string role)
    {
        Role = role.Trim().ToLowerInvariant();
        UpdatedAt = DateTime.UtcNow;
    }
    public void Remove()
    {
        Status = MembershipStatuses.Removed;
        UpdatedAt = DateTime.UtcNow;
    }
    public void Activate()
    {
        Status = MembershipStatuses.Active;
        UpdatedAt = DateTime.UtcNow;
    }
}