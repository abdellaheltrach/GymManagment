using GymManagement.Domain.Bases;
using GymManagement.Domain.Enums;

namespace GymManagement.Domain.Entities;

/// <summary>
/// One cycle of membership for a trainee.
/// A trainee has MANY memberships over time — never model this as one-to-one.
/// Query the active one with:
///   .Where(m => m.TraineeId == id
///            && m.Status == MembershipStatus.Active
///            && m.EndDate > DateTime.UtcNow)
/// </summary>
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

    /// <summary>
    /// Tracks whether the trainee has paid the trainer add-on fee for this
    /// membership cycle. Set to true by PayTrainerAddonCommand.
    /// Once true per cycle, the trainee can switch trainers freely at no cost.
    /// Resets to false when a new membership cycle begins.
    /// </summary>
    public bool TrainerAddonPaid { get; set; } = false;
    public DateTime? TrainerAddonPaidAt { get; set; }
    public decimal TrainerAddonAmountPaid { get; set; } = 0;

    public string? Notes { get; set; }

    // Navigation
    public Trainee Trainee { get; set; } = null!;
    public MembershipPlan Plan { get; set; } = null!;
    public ICollection<Payment> Payments { get; set; } = [];
    public ICollection<FrozenPeriod> FrozenPeriods { get; set; } = [];
}
