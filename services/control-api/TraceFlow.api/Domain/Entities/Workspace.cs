using TraceFlow.Api.Domain.Common;

namespace TraceFlow.Api.Domain.Entities;

public class Workspace : Entity
{
    public string Name { get; private set; } = string.Empty;
    public Ulid OwnerUserId { get; private set; }
    public User Owner { get; private set; } = null!;
    public string Slug { get; private set; } = string.Empty;
    public string? Description { get; private set;} = string.Empty;
    public string Status { get; private set; } = "active";
    public ICollection<WorkspaceMember> Members = new List<WorkspaceMember>();
    private Workspace() {}
    public Workspace(Ulid OwnerUserId, string Name, string Slug, string? Description)
    {
        this.Id = Ulid.NewUlid();
        this.OwnerUserId = OwnerUserId;
        this.Name = Name.Trim();
        this.Slug = Slug.Trim().ToLower();
        this.Description = string.IsNullOrWhiteSpace(Description)
            ? null
            : Description.Trim();
        Status = "active";
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
    public void UpdateWorkspace(string? Name, string? Slug, string? Description)
    {
        if (!string.IsNullOrWhiteSpace(Name))
        {
            this.Name = Name.Trim();
        }
        if (!string.IsNullOrWhiteSpace(Slug))
        {
            this.Slug = Slug.Trim().ToLowerInvariant();
        }
        if (Description is not null)
        {
            this.Description = string.IsNullOrWhiteSpace(Description)
                ? null
                : Description.Trim();
        }
        UpdatedAt = DateTime.UtcNow;
    }
    public void Archive()
    {
        Status = "archived";
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        Status = "active";
        UpdatedAt = DateTime.UtcNow;
    }
}