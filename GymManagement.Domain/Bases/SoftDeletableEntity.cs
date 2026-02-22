namespace GymManagement.Domain.Bases
{
    public abstract class SoftDeletableEntity : BaseEntity
    {
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? DeletedById { get; set; }
    }
}
