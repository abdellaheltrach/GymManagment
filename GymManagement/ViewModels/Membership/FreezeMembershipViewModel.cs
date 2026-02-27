using System.ComponentModel.DataAnnotations;

namespace GymManagement.Web.ViewModels.Memberships
{
    public class FreezeMembershipViewModel
    {
        public Guid MembershipId { get; set; }
        public string TraineeName { get; set; } = string.Empty;
        public int MaxFreezeDaysRemaining { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Freeze From")]
        public DateTime FreezeFrom { get; set; } = DateTime.Today;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Freeze Until")]
        public DateTime FreezeTo { get; set; } = DateTime.Today.AddDays(7);

        [Required]
        [Display(Name = "Reason")]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;
    }

}
