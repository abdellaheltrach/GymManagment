using GymManagement.Domain.Bases;

namespace GymManagement.Domain.Entities
{
    public class Exercise : BaseEntity
    {
        public Guid TrainingProgramId { get; set; }

        public string Name { get; set; } = string.Empty;
        public int Sets { get; set; }
        public int Reps { get; set; }
        public decimal? WeightKg { get; set; }
        public int? DurationSeconds { get; set; }
        public int? RestSeconds { get; set; }
        public int Order { get; set; }
        public string? Notes { get; set; }

        // Navigation
        public TrainingProgram TrainingProgram { get; set; } = null!;
    }
}
