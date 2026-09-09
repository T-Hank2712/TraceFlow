using TraceFlow.Api.Domain.Common;
using TraceFlow.Api.Domain.Constants;

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

    private TraceApplication() {}

    public TraceApplication(
        Ulid projectId,
        Ulid createdByUserId,
        string name,
        string slug,
        string? description)
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
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
    public void Update(string? name, string? slug, string? description)
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

        UpdatedAt = DateTime.UtcNow;
    }
}