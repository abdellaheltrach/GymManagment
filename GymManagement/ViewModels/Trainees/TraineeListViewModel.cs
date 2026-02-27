using GymManagement.Application.Common.DTOs;
using GymManagement.Domain.Enums;
using GymManagement.Web.ViewModels.Shared;

namespace GymManagement.Web.ViewModels.Trainees
{
    public class TraineeListViewModel
    {
        public IReadOnlyList<TraineeSummaryDto> Trainees { get; set; } = [];
        public PaginationViewModel Pagination { get; set; } = new();
        public string? SearchTerm { get; set; }
        public MembershipStatus? StatusFilter { get; set; }
    }

}
