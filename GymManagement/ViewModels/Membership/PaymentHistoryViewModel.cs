using GymManagement.Application.Common.DTOs;

namespace GymManagement.Web.ViewModels.Memberships
{

    public class PaymentHistoryViewModel
    {
        public string TraineeName { get; set; } = string.Empty;
        public MembershipDto Membership { get; set; } = null!;
        public IReadOnlyList<PaymentDto> Payments { get; set; } = [];
    }

}
