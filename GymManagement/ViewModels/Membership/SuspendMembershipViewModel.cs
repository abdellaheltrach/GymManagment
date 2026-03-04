using System.ComponentModel.DataAnnotations;

namespace GymManagement.Web.ViewModels.Memberships;

public class SuspendMembershipViewModel
{
    public Guid   MembershipId { get; set; }
    public Guid   TraineeId    { get; set; }

    [Required(ErrorMessage = "Reason is required.")]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
}
