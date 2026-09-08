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
    public string Status { get; private set; } = WorkspaceMemberStatuses.Active;
    public DateTime JoinedAt { get; private set; }
    private WorkspaceMember() {}
    public WorkspaceMember(Ulid WorkspaceId, Ulid UserId, string Role)
    {
        Id = Ulid.NewUlid();
        this.WorkspaceId = WorkspaceId;
        this.UserId = UserId;
        this.Role = Role.Trim().ToLower();
        Status = WorkspaceMemberStatuses.Active;
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
        Status = WorkspaceMemberStatuses.Removed;
        UpdatedAt = DateTime.UtcNow;
    }
    public void Activate()
    {
        Status = WorkspaceMemberStatuses.Active;
        UpdatedAt = DateTime.UtcNow;
    }
}