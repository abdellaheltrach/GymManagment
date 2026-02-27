using GymManagement.Application.Common.DTOs;
using GymManagement.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace GymManagement.Web.ViewModels.Memberships
{
    public class AssignMembershipViewModel
    {
        public Guid TraineeId { get; set; }
        public string TraineeName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Membership Plan")]
        public Guid PlanId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Display(Name = "Initial Payment")]
        [Range(0, 1000000)]
        public decimal InitialPaymentAmount { get; set; }

        [Required]
        [Display(Name = "Payment Method")]
        public PaymentMethod PaymentMethod { get; set; }

        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        public IReadOnlyList<MembershipPlanDto> AvailablePlans { get; set; } = [];
    }

}
