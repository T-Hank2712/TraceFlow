using TraceFlow.Api.Domain.Common;
using TraceFlow.Api.Domain.Constants;

namespace TraceFlow.Api.Domain.Entities;
public class Project : Entity
{
    public Ulid WorkspaceId { get; private set; }
    public Workspace Workspace { get; private set; } = null!;

    public Ulid CreatedByUserId { get; private set; }
    public User CreatedByUser { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string Status { get; private set; } = ResourceStatuses.Active;
    public ICollection<ProjectMember> Members { get; private set; }
    = new List<ProjectMember>();
    private Project() {}
        public Project(
        Ulid WorkspaceId,
        Ulid CreatedByUserId,
        string Name,
        string Slug,
        string? Description)
    {
        Id = Ulid.NewUlid();
        this.WorkspaceId = WorkspaceId;
        this.CreatedByUserId = CreatedByUserId;
        this.Name = Name.Trim();
        this.Slug = Slug.Trim().ToLowerInvariant();
        this.Description = string.IsNullOrWhiteSpace(Description)
            ? null
            : Description.Trim();
        Status = ResourceStatuses.Active;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}