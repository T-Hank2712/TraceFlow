using TraceFlow.Api.Domain.Common;
using TraceFlow.Api.Domain.Entities;

namespace TraceFlow.Api.Domain.Entities;

public class Workspace : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? Description { get; private set;} = string.Empty;
    public string Status { get; private set; } = "active";
    public ICollection<WorkspaceMember> Members = new List<WorkspaceMember>();
    private Workspace() {}
    public Workspace(string Name, string Slug, string? Description)
    {
        this.Id = Ulid.NewUlid();
        this.Name = Name.Trim();
        this.Slug = Slug.Trim().ToLower();
        this.Description = string.IsNullOrWhiteSpace(Description)
            ? null
            : Description.Trim();
        Status = "active";
        CreatedAt = DateTime.UtcNow;
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