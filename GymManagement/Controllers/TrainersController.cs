using GymManagement.Application.Features.Trainers.Commands.Models;
using GymManagement.Application.Features.Trainers.Queries.Models;
using GymManagement.Domain.Enums;
using GymManagement.Web.Bases;
using GymManagement.Web.Extensions;
using GymManagement.Web.ViewModels.Shared;
using GymManagement.Web.ViewModels.Trainers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Web.Controllers
{
    [Authorize]
    public class TrainersController : BaseController
    {
        // ── List ───────────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Index(
            int page = 1,
            string? search = null,
            TrainerSpecialization? specialization = null,
            CancellationToken ct = default)
        {
            var result = await Mediator.Send(
                new GetTrainersListQuery(page, 20, search, specialization), ct);

            return HandleResult(result, paged => View(new TrainerListViewModel
            {
                Trainers = paged.Items,
                Pagination = new PaginationViewModel
                {
                    CurrentPage = paged.Page,
                    TotalPages = paged.TotalPages,
                    TotalCount = paged.TotalCount,
                    SearchTerm = search,
                    ActionName = nameof(Index),
                    ControllerName = "Trainers"
                }
            }));
        }

        // ── Details ────────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Details(Guid id, CancellationToken ct)
        {
            var result = await Mediator.Send(new GetTrainerByIdQuery(id), ct);
            return HandleResult(result, dto => View(new TrainerDetailViewModel
            {
                Trainer = dto,
                AssignedTrainees = [] // Query for assigned trainees if needed
            }));
        }

        // ── Create ─────────────────────────────────────────────────────────────────
        [HttpGet]
        [Authorize(Policy = "CanManageTrainers")]
        public IActionResult Create() => View(new CreateTrainerViewModel());

        [HttpPost]
        [Authorize(Policy = "CanManageTrainers")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTrainerViewModel vm, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View(vm);

            var result = await Mediator.Send(new RegisterTrainerCommand(
                vm.FirstName, vm.LastName, vm.Email, vm.Phone, vm.DateOfBirth,
                vm.Specialization, vm.Bio, vm.YearsOfExperience,
                vm.SalaryType, vm.BaseSalary, vm.CommissionPerTrainee,
                vm.Password, User.GetUserId()), ct);

            if (result.IsFailure)
            {
                ModelState.AddModelError(string.Empty, result.Error!);
                return View(vm);
            }

            return RedirectWithSuccess(
                $"Trainer {vm.FirstName} {vm.LastName} registered successfully.",
                nameof(Details), new { id = result.Value });
        }
    }
}
