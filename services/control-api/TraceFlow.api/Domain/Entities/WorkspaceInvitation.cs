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
    public string Status { get; private set; } = InvitationStatuses.Pending;
    public DateTimeOffset ExpiresAt { get; private set; }

    private WorkspaceInvitation() { }

    public WorkspaceInvitation(
        Ulid workspaceId,
        Ulid invitedUserId,
        Ulid invitedByUserId,
        string role,
        DateTimeOffset expiresAt,
        DateTimeOffset createdAt)
    {
        Id = Ulid.NewUlid();
        WorkspaceId = workspaceId;
        InvitedUserId = invitedUserId;
        InvitedByUserId = invitedByUserId;
        Role = role.Trim().ToLowerInvariant();
        Status = InvitationStatuses.Pending;
        ExpiresAt = expiresAt;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }
    public void Accept(DateTimeOffset accepted)
    {
        if (Status != InvitationStatuses.Pending)
        {
            throw new InvalidOperationException("Invitation is not pending.");
        }
        if (ExpiresAt <= accepted)
        {
            throw new InvalidOperationException("Invitation has expired.");
        }
        Status = InvitationStatuses.Accepted;
        UpdatedAt = accepted;
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
}
