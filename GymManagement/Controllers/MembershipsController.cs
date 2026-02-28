using GymManagement.Application._Features.MembershipPlans.Queries.Models;
using GymManagement.Application._Features.Memberships.Commands.Models;
using GymManagement.Application._Features.Memberships.Queries.Models;
using GymManagement.Application._Features.Trainees.Queries.Models;
using GymManagement.Domain.Enums;
using GymManagement.Web.Bases;
using GymManagement.Web.Extensions;
using GymManagement.Web.ViewModels.Memberships;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Web.Controllers
{

    [Authorize]
    public class MembershipsController : BaseController
    {
        // ── Assign ─────────────────────────────────────────────────────────────────
        [HttpGet]
        [Authorize(Policy = "CanManageTrainees")]
        public async Task<IActionResult> Assign(Guid traineeId, CancellationToken ct)
        {
            var traineeResult = await Mediator.Send(new GetTraineeByIdQuery(traineeId), ct);
            if (traineeResult.IsFailure) return NotFound();

            var plansResult = await Mediator.Send(new GetActivePlansQuery(), ct);

            return View(new AssignMembershipViewModel
            {
                TraineeId = traineeId,
                TraineeName = traineeResult.Value!.FirstName + " " + traineeResult.Value.LastName,
                StartDate = DateTime.Today,
                AvailablePlans = plansResult.Value ?? []
            });
        }

        [HttpPost]
        [Authorize(Policy = "CanManageTrainees")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(AssignMembershipViewModel vm, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                var plans = await Mediator.Send(new GetActivePlansQuery(), ct);
                vm.AvailablePlans = plans.Value ?? [];
                return View(vm);
            }

            var result = await Mediator.Send(new AssignMembershipCommand(
                vm.TraineeId, vm.PlanId, vm.StartDate,
                vm.InitialPaymentAmount, vm.PaymentMethod,
                User.GetUserId(), vm.Notes), ct);

            if (result.IsFailure)
            {
                ModelState.AddModelError(string.Empty, result.Error!);
                var plans = await Mediator.Send(new GetActivePlansQuery(), ct);
                vm.AvailablePlans = plans.Value ?? [];
                return View(vm);
            }

            return RedirectWithSuccess(
                "Membership assigned successfully.",
                "Details", new { controller = "Trainees", id = vm.TraineeId });
        }

        // ── Renew ──────────────────────────────────────────────────────────────────
        [HttpPost]
        [Authorize(Policy = "CanManageTrainees")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Renew(
            Guid traineeId, Guid planId,
            decimal initialPayment, PaymentMethod method,
            CancellationToken ct)
        {
            var result = await Mediator.Send(new RenewMembershipCommand(
                traineeId, planId, initialPayment, method, User.GetUserId()), ct);

            if (result.IsFailure)
                return RedirectWithError(result.Error!, "Details",
                    new { controller = "Trainees", id = traineeId });

            return RedirectWithSuccess("Membership renewed.",
                "Details", new { controller = "Trainees", id = traineeId });
        }

        // ── Freeze ─────────────────────────────────────────────────────────────────
        [HttpGet]
        [Authorize(Policy = "CanManageTrainees")]
        public async Task<IActionResult> Freeze(Guid traineeId, CancellationToken ct)
        {
            var traineeResult = await Mediator.Send(new GetTraineeByIdQuery(traineeId), ct);
            var membershipResult = await Mediator.Send(new GetActiveMembershipQuery(traineeId), ct);

            if (traineeResult.IsFailure || membershipResult.IsFailure)
                return NotFound();

            var membership = membershipResult.Value!;

            // Fetch the plan to get the real MaxFreezeDays
            var plansResult = await Mediator.Send(new GetActivePlansQuery(), ct);
            var plan = plansResult.Value?.FirstOrDefault(p => p.Name == membership.PlanName);
            var maxFreezeDays = plan?.MaxFreezeDays ?? 0;
            var remainingFreezeDays = Math.Max(0, maxFreezeDays - membership.TotalFrozenDays);

            return View(new FreezeMembershipViewModel
            {
                MembershipId = membership.Id,
                TraineeName = traineeResult.Value!.FirstName + " " + traineeResult.Value.LastName,
                MaxFreezeDaysRemaining = remainingFreezeDays,
                FreezeFrom = DateTime.Today,
                FreezeTo = DateTime.Today.AddDays(7)
            });
        }

        [HttpPost]
        [Authorize(Policy = "CanManageTrainees")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Freeze(FreezeMembershipViewModel vm, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View(vm);

            var result = await Mediator.Send(new FreezeMembershipCommand(
                vm.MembershipId, vm.FreezeFrom, vm.FreezeTo,
                vm.Reason, User.GetUserId()), ct);

            if (result.IsFailure)
            {
                ModelState.AddModelError(string.Empty, result.Error!);
                return View(vm);
            }

            return RedirectWithSuccess("Membership frozen.", "Index", new { controller = "Trainees" });
        }
    }
}
