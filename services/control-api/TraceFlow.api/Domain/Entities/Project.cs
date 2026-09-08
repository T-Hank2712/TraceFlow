using TraceFlow.Api.Domain.Common;

namespace TraceFlow.Api.Domain.Entities;
public class Project : Entity
{
    public Ulid WorkspaceId { get; private set; }
    public Workspace Workspace { get; private set; } = null!;
}