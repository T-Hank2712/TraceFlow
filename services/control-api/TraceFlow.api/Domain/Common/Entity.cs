namespace TraceFlow.Api.Domain.Common
{
    public abstract class Entity
    {
        public Ulid Id { get; set; } = Ulid.NewUlid();
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
