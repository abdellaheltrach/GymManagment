using GymManagement.Application.Common.DTOs;
using GymManagement.Domain.Enums;
using GymManagement.Web.ViewModels.Shared;
using System.ComponentModel.DataAnnotations;

namespace GymManagement.Web.ViewModels.Trainees;

public class TraineeListViewModel
{
    public IReadOnlyList<TraineeSummaryDto> Trainees { get; set; } = [];
    public PaginationViewModel Pagination { get; set; } = new();
    public string? SearchTerm { get; set; }
    public MembershipStatus? StatusFilter { get; set; }
}

public class TraineeDetailViewModel
{
    public TraineeDetailDto Trainee { get; set; } = null!;

    // Display helpers
    public string MembershipStatusBadge => Trainee.ActiveMembership?.Status switch
    {
        MembershipStatus.Active         => "badge bg-success",
        MembershipStatus.Frozen         => "badge bg-info",
        MembershipStatus.Expired        => "badge bg-danger",
        MembershipStatus.PendingPayment => "badge bg-warning",
        MembershipStatus.Suspended      => "badge bg-secondary",
        MembershipStatus.Cancelled      => "badge bg-dark",
        _                               => "badge bg-light text-dark"
    };

    public string MembershipStatusText => Trainee.ActiveMembership?.Status switch
    {
        MembershipStatus.Active         => "Active",
        MembershipStatus.Frozen         => "Frozen",
        MembershipStatus.Expired        => "Expired",
        MembershipStatus.PendingPayment => "Pending Payment",
        MembershipStatus.Suspended      => "Suspended",
        MembershipStatus.Cancelled      => "Cancelled",
        _                               => "No Membership"
    };
}

public class RegisterTraineeViewModel
{
    [Required] [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required] [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [Required] [EmailAddress] [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required] [Display(Name = "Phone")]
    public string Phone { get; set; } = string.Empty;

    [Required] [Display(Name = "National ID")]
    public string NationalId { get; set; } = string.Empty;

    [Required] [DataType(DataType.Date)] [Display(Name = "Date of Birth")]
    public DateTime DateOfBirth { get; set; }

    [Required] [Display(Name = "Gender")]
    public Gender Gender { get; set; }

    [Display(Name = "Address")]
    public string? Address { get; set; }

    [Display(Name = "Medical Notes")]
    public string? MedicalNotes { get; set; }

    [Display(Name = "Height (cm)")]
    [Range(50, 300)]
    public decimal? HeightCm { get; set; }

    [Display(Name = "Weight (kg)")]
    [Range(20, 500)]
    public decimal? WeightKg { get; set; }

    [Required] [Display(Name = "Emergency Contact Name")]
    public string EmergencyContactName { get; set; } = string.Empty;

    [Required] [Display(Name = "Emergency Contact Phone")]
    public string EmergencyContactPhone { get; set; } = string.Empty;

    [Display(Name = "Relation")]
    public string? EmergencyContactRelation { get; set; }
}

public class EditTraineeViewModel : RegisterTraineeViewModel
{
    public Guid TraineeId { get; set; }
}

public class AssignTrainerViewModel
{
    public Guid    TraineeId           { get; set; }
    public string  TraineeName         { get; set; } = string.Empty;
    public Guid?   MembershipId        { get; set; }
    public string? CurrentTrainerName  { get; set; }
    public string  PlanName            { get; set; } = string.Empty;
    public bool    PlanIncludesTrainer { get; set; }
    public bool    HasActiveMembership { get; set; }
    public bool    TrainerAddonPaid    { get; set; }
    public decimal TrainerAddonFee     { get; set; }

    [Required(ErrorMessage = "Please select a trainer.")]
    public Guid SelectedTrainerId { get; set; }

    public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> AvailableTrainers
        { get; set; } = [];

    // ── Upgrade Scenario ──────────────────────────────────────────────────
    public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> EligiblePlans { get; set; } = [];
    public Guid? NewPlanId { get; set; }
    public PaymentMethod UpgradePaymentMethod { get; set; } = PaymentMethod.Cash;

    public bool IsReassignment  => CurrentTrainerName is not null;
    public bool CanAssign       => HasActiveMembership && PlanIncludesTrainer && TrainerAddonPaid;
    public bool AddonFeeIsZero  => TrainerAddonFee == 0;
}

public class PayTrainerAddonViewModel
{
    public Guid TraineeId { get; set; }

    [Required]
    [Range(0, 100000, ErrorMessage = "Enter a valid amount.")]
    public decimal Amount { get; set; }

    [Required]
    public GymManagement.Domain.Enums.PaymentMethod Method { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}
