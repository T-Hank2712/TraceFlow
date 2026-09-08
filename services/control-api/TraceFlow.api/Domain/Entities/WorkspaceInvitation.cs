using TraceFlow.Api.Domain.Common;
using TraceFlow.Api.Domain.Constants;

namespace TraceFlow.Api.Domain.Entities;

public class WorkspaceInvitation : Entity
{
    public Ulid WorkspaceId { get; private set; }
    public Workspace Workspace { get; private set; } = null!;

    public Ulid InvitedUserId { get; private set; }
    public User InvitedUser { get; private set; } = null!;

    public Ulid InvitedByUserId { get; private set; }
    public User InvitedByUser { get; private set; } = null!;

    public string Role { get; private set; } = WorkspaceMemberRoles.Member;
    public string Status { get; private set; } = WorkspaceInvitationStatuses.Pending;
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
        Status = WorkspaceInvitationStatuses.Pending;
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
    public void Accept()
    {
        if (Status != WorkspaceInvitationStatuses.Pending)
        {
            throw new InvalidOperationException("Invitation is not pending.");
        }
        if (ExpiresAt <= DateTime.UtcNow)
        {
            throw new InvalidOperationException("Invitation has expired.");
        }
        Status = WorkspaceInvitationStatuses.Accepted;
        UpdatedAt = DateTime.UtcNow;
    }
    public void Decline()
    {
        if (Status != WorkspaceInvitationStatuses.Pending)
        {
            throw new InvalidOperationException("Invitation is not pending.");
        }

        if (ExpiresAt <= DateTime.UtcNow)
        {
            throw new InvalidOperationException("Invitation has expired.");
        }

        Status = WorkspaceInvitationStatuses.Declined;
        UpdatedAt = DateTime.UtcNow;
    }
}