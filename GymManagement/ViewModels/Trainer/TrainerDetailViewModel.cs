using GymManagement.Application.Common.DTOs;

namespace GymManagement.Web.ViewModels.Trainers
{
    public class TrainerDetailViewModel
    {
        public TrainerDetailDto Trainer { get; set; } = null!;
        public IReadOnlyList<TraineeSummaryDto> AssignedTrainees { get; set; } = [];
    }
}
