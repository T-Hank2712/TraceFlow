namespace TraceFlow.api.Domain.Common
{
    public abstract class Entity
    {
        public Ulid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}