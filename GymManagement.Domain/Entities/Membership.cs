using GymManagement.Domain.Bases;
using GymManagement.Domain.Enums;

namespace GymManagement.Domain.Entities
{
    public class Membership : AuditableEntity
    {
        public Guid TraineeId { get; set; }
        public Guid PlanId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        /// <summary>Accumulated frozen days — used to extend EndDate when freeze ends.</summary>
        public int TotalFrozenDays { get; set; } = 0;

        public MembershipStatus Status { get; set; } = MembershipStatus.PendingPayment;

        public decimal TotalAmount { get; set; }

        /// <summary>Kept in sync by RecordPayment command via Unit of Work.</summary>
        public decimal AmountPaid { get; set; } = 0;

        public decimal RemainingBalance => TotalAmount - AmountPaid;
        public bool IsFullyPaid => RemainingBalance <= 0;

        public string? Notes { get; set; }

        // Navigation
        public Trainee Trainee { get; set; } = null!;
        public MembershipPlan Plan { get; set; } = null!;
        public ICollection<Payment> Payments { get; set; } = [];
        public ICollection<FrozenPeriod> FrozenPeriods { get; set; } = [];
    }
}
