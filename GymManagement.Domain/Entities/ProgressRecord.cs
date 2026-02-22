using GymManagement.Domain.Bases;

namespace GymManagement.Domain.Entities
{
    public class ProgressRecord : BaseEntity
    {
        public Guid TraineeId { get; set; }
        public string RecordedById { get; set; } = string.Empty;

        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
        public decimal? WeightKg { get; set; }
        public decimal? BodyFatPercent { get; set; }
        public decimal? MuscleMassKg { get; set; }
        public decimal? ChestCm { get; set; }
        public decimal? WaistCm { get; set; }
        public decimal? HipsCm { get; set; }
        public decimal? ArmCm { get; set; }
        public string? Notes { get; set; }

        // Navigation
        public Trainee Trainee { get; set; } = null!;
    }
}
