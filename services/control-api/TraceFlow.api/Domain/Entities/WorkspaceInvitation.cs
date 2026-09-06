using TraceFlow.Api.Domain.Common;

namespace TraceFlow.Api.Domain.Entities;

public class WorkspaceInvitation : Entity
{
    public Ulid WorkspaceId { get; private set; }
    public Workspace Workspace { get; private set; } = null!;

    public Ulid InvitedUserId { get; private set; }
    public User InvitedUser { get; private set; } = null!;

    public Ulid InvitedByUserId { get; private set; }
    public User InvitedByUser { get; private set; } = null!;

    public string Role { get; private set; } = "member";
    public string Status { get; private set; } = "pending";
    public DateTime ExpiresAt { get; private set; }

    private WorkspaceInvitation() {}

    public WorkspaceInvitation(
        Ulid workspaceId,
        Ulid invitedUserId,
        Ulid invitedByUserId,
        string role,
        DateTime expiresAt)
    {
        Id = Ulid.NewUlid();
        WorkspaceId = workspaceId;
        InvitedUserId = invitedUserId;
        InvitedByUserId = invitedByUserId;
        Role = role.Trim().ToLowerInvariant();
        Status = "pending";
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}