using GymManagement.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace GymManagement.Web.ViewModels.Memberships
{
    public class UpdatePaymentViewModel
    {
        public Guid PaymentId { get; set; }
        public string TraineeName { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal AlreadyPaid { get; set; }
        public decimal RemainingBalance { get; set; }

        [Required]
        [Display(Name = "Additional Amount")]
        [Range(0.01, 1000000)]
        public decimal AdditionalAmount { get; set; }

        [Required]
        [Display(Name = "Payment Method")]
        public PaymentMethod Method { get; set; }

        [Display(Name = "Notes")]
        public string? Notes { get; set; }
    }
}
