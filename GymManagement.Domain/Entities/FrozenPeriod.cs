using GymManagement.Domain.Bases;

namespace GymManagement.Domain.Entities
{
    public class FrozenPeriod : BaseEntity
    {
        public Guid MembershipId { get; set; }

        public DateTime FrozenFrom { get; set; }
        public DateTime FrozenTo { get; set; }

        public int DurationDays => (FrozenTo - FrozenFrom).Days;

        public string Reason { get; set; } = string.Empty;
        public string RequestedById { get; set; } = string.Empty;

        // Navigation
        public Membership Membership { get; set; } = null!;
    }
}
