using GymManagement.Domain.Bases;
using GymManagement.Domain.Enums;

namespace GymManagement.Domain.Entities
{
    public class Attendance : BaseEntity
    {
        public Guid TraineeId { get; set; }

        public DateTime CheckInTime { get; set; } = DateTime.UtcNow;
        public DateTime? CheckOutTime { get; set; }

        public AttendanceMethod Method { get; set; } = AttendanceMethod.Manual;
        public string RecordedById { get; set; } = string.Empty;
        public string? Notes { get; set; }

        public TimeSpan? Duration => CheckOutTime.HasValue
            ? CheckOutTime.Value - CheckInTime
            : null;

        // Navigation
        public Trainee Trainee { get; set; } = null!;
    }
}
