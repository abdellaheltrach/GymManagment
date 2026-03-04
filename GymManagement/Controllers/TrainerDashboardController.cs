using GymManagement.Application.Features.Dashboard.Queries.Models;
using GymManagement.Domain.Interfaces;
using GymManagement.Web.Bases;
using GymManagement.Web.ViewModels.TrainerDashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Web.Controllers
{
    [Authorize(Roles = "Trainer")]
    public class TrainerDashboardController : BaseController
    {
        private readonly IUnitOfWork _uow;

        public TrainerDashboardController(IUnitOfWork uow) => _uow = uow;

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            // Resolve domain TrainerId from the logged-in Identity UserId
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Forbid();

            var trainer = await _uow.Trainers.FindAsync(
                t => t.ApplicationUserId == userId, ct);

            var domainTrainer = trainer.FirstOrDefault();
            if (domainTrainer is null)
                return NotFound();

            var result = await Mediator.Send(
                new GetTrainerDashboardQuery(domainTrainer.Id), ct);

            return HandleResult(result, data => View(new TrainerDashboardViewModel
            {
                Data = data,
                TrainerName = domainTrainer.FullName,
                Specialization = domainTrainer.Specialization.ToString(),
                TrainerId = domainTrainer.Id
            }));
        }
    }
}
