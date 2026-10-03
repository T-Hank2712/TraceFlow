namespace TraceFlow.Api.Domain.Entities;

public class TraceApplication : Entity
{
    public Ulid ProjectId { get; private set; }
    public Project Project { get; private set; } = null!;

    public Ulid CreatedByUserId { get; private set; }
    public User CreatedByUser { get; private set; } = null!;

    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string Status { get; private set; } = ResourceStatuses.Active;
    public ICollection<ApiKey> ApiKeys { get; private set; } = new List<ApiKey>();

    private TraceApplication() { }

    public TraceApplication(
        Ulid projectId,
        Ulid createdByUserId,
        string name,
        string slug,
        string? description,
        DateTimeOffset createdAt)
    {
        Id = Ulid.NewUlid();
        ProjectId = projectId;
        CreatedByUserId = createdByUserId;
        Name = name.Trim();
        Slug = slug.Trim().ToLowerInvariant();
        Description = string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();
        Status = ResourceStatuses.Active;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }
    public void Update(
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
        if (Status == ResourceStatuses.Archived)
        {
            return;
        }

        Status = ResourceStatuses.Archived;
        UpdatedAt = archivedAt;
    }
}
