using GymManagement.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace GymManagement.Web.ViewModels.Memberships
{
    public class RecordPaymentViewModel
    {
        public Guid MembershipId { get; set; }
        public string TraineeName { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal RemainingBalance { get; set; }

        [Required]
        [Display(Name = "Amount")]
        [Range(0.01, 1000000)]
        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "Payment Method")]
        public PaymentMethod Method { get; set; }

        [Display(Name = "Notes")]
        public string? Notes { get; set; }
    }

}
