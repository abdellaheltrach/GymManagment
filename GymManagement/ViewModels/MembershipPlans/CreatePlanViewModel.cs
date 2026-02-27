using GymManagement.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace GymManagement.Web.ViewModels.MembershipPlans
{
    public class CreatePlanViewModel
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Range(1, 3650)]
        public int DurationDays { get; set; }

        [Range(1, 100000)]
        public decimal Price { get; set; }

        public AccessLevel AccessLevel { get; set; }

        public bool IncludesPersonalTrainer { get; set; }

        [Range(0, 365)]
        public int MaxFreezeDays { get; set; }
    }
}
