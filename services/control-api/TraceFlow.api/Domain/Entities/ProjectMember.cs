using TraceFlow.Api.Domain.Common;
using TraceFlow.Api.Domain.Constants;

namespace TraceFlow.Api.Domain.Entities;

public class ProjectMember : Entity
{
    public Ulid ProjectId { get; private set; }
    public Project Project { get; private set; } = null!;

    public Ulid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public string Role { get; private set; } = ProjectMemberRoles.Viewer;
    public string Status { get; private set; } = MembershipStatuses.Active;
    public DateTime JoinedAt { get; private set; }

    private ProjectMember() {}

    public ProjectMember(
        Ulid projectId,
        Ulid userId,
        string role)
    {
        Id = Ulid.NewUlid();
        ProjectId = projectId;
        UserId = userId;
        Role = role.Trim().ToLowerInvariant();
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

    public void Activate()
    {
        Status = MembershipStatuses.Active;
        UpdatedAt = DateTime.UtcNow;
    }
}