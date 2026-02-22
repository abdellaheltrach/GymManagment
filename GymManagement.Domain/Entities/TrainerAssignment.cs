using GymManagement.Domain.Bases;

namespace GymManagement.Domain.Entities
{
    public class TrainerAssignment : BaseEntity
    {
        public Guid TrainerId { get; set; }
        public Guid TraineeId { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public string AssignedById { get; set; } = string.Empty;

        public DateTime? RemovedAt { get; set; }
        public string? RemovedById { get; set; }
        public string? RemovalReason { get; set; }

        public bool IsActive => RemovedAt is null;

        // Navigation
        public Trainer Trainer { get; set; } = null!;
        public Trainee Trainee { get; set; } = null!;
    }
}
