using GymManagement.Domain.Bases;
using GymManagement.Domain.Enums;

namespace GymManagement.Domain.Entities
{
    public class Notification : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; }

        /// When null, notification is sent immediately. Otherwise scheduled for this time.
        public DateTime? ScheduledFor { get; set; }

        public bool IsSent { get; set; } = false;
        public DateTime? SentAt { get; set; }

        // Navigation
        public Trainee? Trainee { get; set; }
    }
}
