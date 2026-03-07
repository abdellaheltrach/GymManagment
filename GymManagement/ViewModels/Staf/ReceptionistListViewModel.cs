using GymManagement.Domain.Enums;
using GymManagement.Web.ViewModels.Shared;
using System.Numerics;

namespace GymManagement.Web.ViewModels.Staf
{
    public class ReceptionistListViewModel
    {
        public IReadOnlyList<ReceptionistSummaryVm> Items { get; set; } = [];
        public string? SearchTerm { get; set; }
        public bool? FilterActive { get; set; }
        public PaginationViewModel Pagination { get; set; } = new();
    }

    public class ReceptionistSummaryVm
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public DateTime HireDate { get; set; }
        public bool IsActive { get; set; }
        public ReceptionistPermission Permissions { get; set; }

        public string StatusBadge => IsActive ? "badge bg-success" : "badge bg-secondary";
        public string StatusLabel => IsActive ? "Active" : "Inactive";

        /// <summary>Count of set permission bits via portable BitOperations.PopCount.</summary>
        public int PermCount => BitOperations.PopCount((uint)(int)Permissions);
    }
}