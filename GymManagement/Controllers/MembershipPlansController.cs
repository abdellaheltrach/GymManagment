using GymManagement.Application._Features.MembershipPlans.Commands.Models;
using GymManagement.Application._Features.MembershipPlans.Queries.Models;
using GymManagement.Web.Bases;
using GymManagement.Web.ViewModels.MembershipPlans;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Web.Controllers
{

    [Authorize(Policy = "CanManagePlans")]
    public class MembershipPlansController : BaseController
    {
        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var result = await Mediator.Send(new GetActivePlansQuery(), ct);
            return HandleResult(result, plans => View(plans));
        }

        [HttpGet]
        public IActionResult Create() => View(new CreatePlanViewModel());

        [HttpPost]
        [Authorize(Policy = "CanManagePlans")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePlanViewModel vm, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View(vm);

            var result = await Mediator.Send(new CreatePlanCommand(
                vm.Name, vm.Description, vm.DurationDays, vm.Price,
                vm.AccessLevel, vm.IncludesPersonalTrainer, vm.MaxFreezeDays), ct);

            if (result.IsFailure)
            {
                ModelState.AddModelError(string.Empty, result.Error!);
                return View(vm);
            }

            return RedirectWithSuccess("Plan created.", nameof(Index));
        }

        [HttpPost]
        [Authorize(Policy = "CanManagePlans")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
        {
            var result = await Mediator.Send(new DeactivatePlanCommand(id), ct);

            if (result.IsFailure)
                return RedirectWithError(result.Error!, nameof(Index));

            return RedirectWithSuccess("Plan deactivated.", nameof(Index));
        }
    }
}

