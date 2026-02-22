using GymManagement.Domain.Bases;
using GymManagement.Domain.Enums;

namespace GymManagement.Domain.Entities
{
    public class AuditLog : BaseEntity
    {
        public string EntityName { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public AuditAction Action { get; set; }

        /// JSON snapshot of values before the change. Null on Created.
        public string? OldValues { get; set; }

        /// JSON snapshot of values after the change. Null on Deleted.
        public string? NewValues { get; set; }

        public string? ChangedById { get; set; }
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    }
}
