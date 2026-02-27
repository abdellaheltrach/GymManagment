using GymManagement.Application.Common.DTOs;
using GymManagement.Web.ViewModels.Shared;

namespace GymManagement.Web.ViewModels.Trainers
{
    public class TrainerListViewModel
    {
        public IReadOnlyList<TrainerSummaryDto> Trainers { get; set; } = [];
        public PaginationViewModel Pagination { get; set; } = new();
    }
}
