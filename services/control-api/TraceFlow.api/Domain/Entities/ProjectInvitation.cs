using TraceFlow.Api.Domain.Common;
using TraceFlow.Api.Domain.Constants;

namespace TraceFlow.Api.Domain.Entities;

public class ProjectInvitation : Entity
{
    public Ulid WorkspaceId { get; private set; }
    public Workspace Workspace { get; private set; } = null!;

    public Ulid ProjectId { get; private set; }
    public Project Project { get; private set; } = null!;

    public Ulid InvitedUserId { get; private set; }
    public User InvitedUser { get; private set; } = null!;

    public Ulid InvitedByUserId { get; private set; }
    public User InvitedByUser { get; private set; } = null!;

    public string Role { get; private set; } = ProjectMemberRoles.Viewer;
    public string Status { get; private set; } = InvitationStatuses.Pending;
    public DateTime ExpiresAt { get; private set; }

    private ProjectInvitation() {}

    public ProjectInvitation(
        Ulid workspaceId,
        Ulid projectId,
        Ulid invitedUserId,
        Ulid invitedByUserId,
        string role,
        DateTime expiresAt)
    {
        Id = Ulid.NewUlid();
        WorkspaceId = workspaceId;
        ProjectId = projectId;
        InvitedUserId = invitedUserId;
        InvitedByUserId = invitedByUserId;
        Role = role.Trim().ToLowerInvariant();
        Status = InvitationStatuses.Pending;
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Accept()
    {
        if (Status != InvitationStatuses.Pending)
        {
            throw new InvalidOperationException("Invitation is not pending.");
        }

        if (ExpiresAt <= DateTime.UtcNow)
        {
            throw new InvalidOperationException("Invitation has expired.");
        }

        Status = InvitationStatuses.Accepted;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Decline()
    {
        if (Status != InvitationStatuses.Pending)
        {
            throw new InvalidOperationException("Invitation is not pending.");
        }

        if (ExpiresAt <= DateTime.UtcNow)
        {
            throw new InvalidOperationException("Invitation has expired.");
        }

        Status = InvitationStatuses.Declined;
        UpdatedAt = DateTime.UtcNow;
    }
    public void Cancel()
    {
        if (Status != InvitationStatuses.Pending)
        {
            throw new InvalidOperationException("Only pending invitation can be cancelled.");
        }

        Status = InvitationStatuses.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }
}