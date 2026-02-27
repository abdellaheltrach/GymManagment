using GymManagement.Application.Common.DTOs;
using System.Globalization;

namespace GymManagement.Web.ViewModels.Dashboard
{
    public class AdminDashboardViewModel
    {
        public AdminDashboardDto Data { get; set; } = null!;

        // Formatted display helpers
        public string FormattedRevenue => Data.MonthlyRevenue.ToString("C", CultureInfo.CurrentCulture);
        public string CurrentMonthName => DateTime.UtcNow.ToString("MMMM yyyy");
    }
}
