namespace TraceFlow.Api.Domain.Entities;

public class Workspace : Entity
{
    public string Name { get; private set; } = string.Empty;
    public Ulid OwnerUserId { get; private set; }
    public User Owner { get; private set; } = null!;
    public string Slug { get; private set; } = string.Empty;
    public string? Description { get; private set; } = string.Empty;
    public string Status { get; private set; } = ResourceStatuses.Active;
    public ICollection<WorkspaceMember> Members = new List<WorkspaceMember>();
    public ICollection<Project> Projects { get; private set; } = new List<Project>();
    private Workspace() { }
    public Workspace(
        Ulid ownerUserId,
        string name,
        string slug,
        string? description,
        DateTimeOffset createdAt)
    {
        Id = Ulid.NewUlid();
        OwnerUserId = ownerUserId;
        Name = name.Trim();
        Slug = slug.Trim().ToLower();
        Description = string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();
        Status = ResourceStatuses.Active;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }
    public void UpdateWorkspace(
        string? name,
        string? slug,
        string? description,
        DateTimeOffset updatedAt)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            Name = name.Trim();
        }
        if (!string.IsNullOrWhiteSpace(slug))
        {
            Slug = slug.Trim().ToLowerInvariant();
        }
        if (description is not null)
        {
            Description = string.IsNullOrWhiteSpace(description)
                ? null
                : description.Trim();
        }
        UpdatedAt = updatedAt;
    }
    public void Archive(DateTimeOffset archivedAt)
    {
        Status = ResourceStatuses.Archived;
        UpdatedAt = archivedAt;
    }

    public void Activate(DateTimeOffset activatedAt)
    {
        Status = ResourceStatuses.Active;
        UpdatedAt = activatedAt;
    }
    public void TransferOwnership(Ulid newOwnerUserId, DateTimeOffset transferredAt)
    {
        OwnerUserId = newOwnerUserId;
        UpdatedAt = transferredAt;
    }
}
