using GymManagement.Application.Common.DTOs;

namespace GymManagement.Web.ViewModels.TrainerDashboard
{
    public class TrainerDashboardViewModel
    {
        public required TrainerDashboardDto Data { get; init; }
        public required string TrainerName { get; init; }
        public required string Specialization { get; init; }
        public required Guid TrainerId { get; init; }

        public string GreetingTime => DateTime.Now.Hour switch
        {
            < 12 => "Good morning",
            < 17 => "Good afternoon",
            _ => "Good evening"
        };

        public string FormattedCommission =>
            Data.CommissionThisMonth.HasValue
                ? $"{Data.CommissionThisMonth.Value:C}"
                : "Fixed salary";
    }

}
