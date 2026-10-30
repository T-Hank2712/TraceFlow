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
    public DateTimeOffset ExpiresAt { get; private set; }

    private ProjectInvitation() { }

    public ProjectInvitation(
        Ulid workspaceId,
        Ulid projectId,
        Ulid invitedUserId,
        Ulid invitedByUserId,
        string role,
        DateTimeOffset expiresAt,
        DateTimeOffset createdAt)
    {
        Id = Ulid.NewUlid();
        WorkspaceId = workspaceId;
        ProjectId = projectId;
        InvitedUserId = invitedUserId;
        InvitedByUserId = invitedByUserId;
        Role = role.Trim().ToLowerInvariant();
        Status = InvitationStatuses.Pending;
        ExpiresAt = expiresAt;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public void Accept(DateTimeOffset acceptedAt)
    {
        if (Status != InvitationStatuses.Pending)
        {
            throw new InvalidOperationException("Invitation is not pending.");
        }

        if (ExpiresAt <= acceptedAt)
        {
            throw new InvalidOperationException("Invitation has expired.");
        }

        Status = InvitationStatuses.Accepted;
        UpdatedAt = acceptedAt;
    }

    public void Decline(DateTimeOffset declined)
    {
        if (Status != InvitationStatuses.Pending)
        {
            throw new InvalidOperationException("Invitation is not pending.");
        }

        if (ExpiresAt <= declined)
        {
            throw new InvalidOperationException("Invitation has expired.");
        }

        Status = InvitationStatuses.Declined;
        UpdatedAt = declined;
    }
    public void Cancel(DateTimeOffset cancelled)
    {
        if (Status != InvitationStatuses.Pending)
        {
            throw new InvalidOperationException("Only pending invitation can be cancelled.");
        }

        Status = InvitationStatuses.Cancelled;
        UpdatedAt = cancelled;
    }
    public void Expire(DateTimeOffset expiredAt)
    {
        if (Status != InvitationStatuses.Pending)
        {
            return;
        }

        if (ExpiresAt > expiredAt)
        {
            return;
        }

        Status = InvitationStatuses.Expired;
        UpdatedAt = expiredAt;
    }
}
