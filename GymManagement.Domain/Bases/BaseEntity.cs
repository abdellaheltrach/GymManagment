namespace GymManagement.Domain.Bases
{
    public abstract class BaseEntity
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedById { get; set; }
        public string? UpdatedById { get; set; }
    }
}
