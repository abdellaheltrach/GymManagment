using GymManagement.Domain.Bases;
using GymManagement.Domain.Enums;

namespace GymManagement.Domain.Entities
{
    public class Payment : AuditableEntity
    {
        public Guid MembershipId { get; set; }
        public string RecordedById { get; set; } = string.Empty;

        public decimal Amount { get; set; }
        public PaymentMethod Method { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Paid;
        public DateTime PaidAt { get; set; } = DateTime.UtcNow;
        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }

        // Refund
        public bool IsRefunded { get; set; } = false;
        public DateTime? RefundedAt { get; set; }
        public string? RefundedById { get; set; }
        public string? RefundReason { get; set; }

        // Navigation
        public Membership Membership { get; set; } = null!;
    }
}
