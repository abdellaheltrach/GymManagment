using GymManagement.Application.Common.DTOs;
using GymManagement.Domain.Enums;

namespace GymManagement.Web.ViewModels.Trainees
{
    public class TraineeDetailViewModel
    {
        public TraineeDetailDto Trainee { get; set; } = null!;

        // Display helpers
        public string MembershipStatusBadge => Trainee.ActiveMembership?.Status switch
        {
            MembershipStatus.Active => "badge bg-success",
            MembershipStatus.Frozen => "badge bg-info",
            MembershipStatus.Expired => "badge bg-danger",
            MembershipStatus.PendingPayment => "badge bg-warning",
            MembershipStatus.Suspended => "badge bg-secondary",
            _ => "badge bg-light text-dark"
        };

        public string MembershipStatusText => Trainee.ActiveMembership?.Status switch
        {
            MembershipStatus.Active => "Active",
            MembershipStatus.Frozen => "Frozen",
            MembershipStatus.Expired => "Expired",
            MembershipStatus.PendingPayment => "Pending Payment",
            MembershipStatus.Suspended => "Suspended",
            _ => "No Membership"
        };
    }

}
