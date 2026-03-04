using System.ComponentModel.DataAnnotations;

namespace GymManagement.Web.ViewModels.Memberships;

public class UnsuspendMembershipViewModel
{
    public Guid   MembershipId { get; set; }
    public Guid   TraineeId    { get; set; }

    [MaxLength(500)]
    public string Notes { get; set; } = string.Empty;
}
