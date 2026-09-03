namespace TraceFlow.Api.Domain.Common
{
    public abstract class Entity
    {
        public Ulid Id { get; set; } = Ulid.NewUlid();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}