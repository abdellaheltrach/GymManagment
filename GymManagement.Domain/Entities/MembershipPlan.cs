using GymManagement.Domain.Bases;
using GymManagement.Domain.Enums;

namespace GymManagement.Domain.Entities;

public class MembershipPlan : SoftDeletableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DurationDays { get; set; }
    public decimal Price { get; set; }
    public AccessLevel AccessLevel { get; set; }
    public bool IncludesPersonalTrainer { get; set; } = false;

    /// <summary>
    /// One-time fee per membership cycle to unlock personal trainer access.
    /// 0 = included in plan price (no extra charge).
    /// Only relevant when IncludesPersonalTrainer = true.
    /// </summary>
    public decimal TrainerAddonFee { get; set; } = 0;

    /// <summary>Max days this plan allows freezing per cycle. 0 = not allowed.</summary>
    public int MaxFreezeDays { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<Membership> Memberships { get; set; } = [];
}
