using GymManagement.Domain.Bases;

namespace GymManagement.Domain.Entities
{

    public class TrainingProgram : SoftDeletableEntity
    {
        public Guid TrainerId { get; set; }
        public Guid TraineeId { get; set; }

        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation
        public Trainer Trainer { get; set; } = null!;
        public Trainee Trainee { get; set; } = null!;
        public ICollection<Exercise> Exercises { get; set; } = [];
    }
}
