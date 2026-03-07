using GymManagement.Application.Features.MembershipPlans.Queries.Models;
using GymManagement.Application.Features.Memberships.Commands.CancelMembership;
using GymManagement.Application.Features.Memberships.Commands.Models;
using GymManagement.Application.Features.Memberships.Commands.SuspendMembership;
using GymManagement.Application.Features.Memberships.Queries.Models;
using GymManagement.Application.Features.Trainees.Queries.Models;
using GymManagement.Domain.Enums;
using GymManagement.Web.Bases;
using GymManagement.Web.Extensions;
using GymManagement.Web.ViewModels.Memberships;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Web.Controllers;

[Authorize]
public class MembershipsController : BaseController
{
    // ── Assign ─────────────────────────────────────────────────────────────────
    [HttpGet]
    [Authorize(Policy = "CanAssignMemberships")]
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

    // POST /Memberships/Assign
    [HttpPost]
    [Authorize(Policy = "CanAssignMemberships")]
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
            User.GetUserId(), vm.Notes ?? string.Empty), ct);

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
    // POST /Memberships/Renew
    [HttpPost]
    [Authorize(Policy = "CanAssignMemberships")]
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
    // GET /Memberships/Freeze?traineeId={guid}
    [HttpGet]
    [Authorize(Policy = "CanFreezeMemberships")]
    public async Task<IActionResult> Freeze(Guid traineeId, CancellationToken ct)
    {
        var traineeResult = await Mediator.Send(new GetTraineeByIdQuery(traineeId), ct);
        var membershipResult = await Mediator.Send(new GetActiveMembershipQuery(traineeId), ct);

        if (traineeResult.IsFailure || membershipResult.IsFailure)
            return NotFound();

        var membership = membershipResult.Value!;

        // Fetch the plan to get the real MaxFreezeDays remaining
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

    // POST /Memberships/Freeze
    [HttpPost]
    [Authorize(Policy = "CanFreezeMemberships")]
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

    // ── Cancel ─────────────────────────────────────────────────────────────────
    // GET /Memberships/Cancel?traineeId={guid}
    [HttpGet]
    [Authorize(Roles = "CanCancelMemberships")]
    public async Task<IActionResult> Cancel(Guid traineeId, CancellationToken ct)
    {
        var traineeResult = await Mediator.Send(new GetTraineeByIdQuery(traineeId), ct);
        var membershipResult = await Mediator.Send(new GetActiveMembershipQuery(traineeId), ct);

        if (traineeResult.IsFailure || membershipResult.IsFailure)
            return NotFound();

        var membership = membershipResult.Value!;
        var trainee = traineeResult.Value!;

        // Suggest refund based on unused days
        var totalDays = (membership.EndDate - membership.StartDate).TotalDays;
        var usedDays = (DateTime.UtcNow - membership.StartDate).TotalDays;
        var unusedDays = Math.Max(0, totalDays - usedDays);
        var suggested = totalDays > 0
            ? Math.Round(membership.AmountPaid * (decimal)(unusedDays / totalDays), 2)
            : 0;

        return View(new CancelMembershipViewModel
        {
            MembershipId = membership.Id,
            TraineeId = traineeId,
            TraineeName = trainee.FirstName + " " + trainee.LastName,
            PlanName = membership.PlanName,
            AmountPaid = membership.AmountPaid,
            SuggestedRefund = suggested,
            RefundMethod = PaymentMethod.Cash
        });
    }

    // POST /Memberships/Cancel
    [HttpPost]
    [Authorize(Roles = "CanCancelMemberships")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(CancelMembershipViewModel vm, CancellationToken ct)
    {
        // Partial refund requires a positive amount
        if (vm.RefundType == RefundType.Partial &&
            (vm.RefundAmount is null or <= 0))
        {
            ModelState.AddModelError(nameof(vm.RefundAmount),
                "Please enter the refund amount.");
        }

        if (!ModelState.IsValid) return View(vm);

        var result = await Mediator.Send(new CancelMembershipCommand(
            vm.MembershipId,
            vm.RefundType,
            vm.RefundType == RefundType.Partial ? vm.RefundAmount ?? 0 : 0,
            vm.RefundMethod,
            vm.Reason,
            User.GetUserId()), ct);

        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(vm);
        }

        return RedirectWithSuccess(
            "Membership cancelled successfully.",
            "Details", new { controller = "Trainees", id = vm.TraineeId });
    }

    // ── Suspend ────────────────────────────────────────────────────────────────
    // POST /Memberships/Suspend
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Suspend(SuspendMembershipViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return RedirectWithError("Reason is required to suspend.",
                "Details", new { controller = "Trainees", id = vm.TraineeId });

        var result = await Mediator.Send(new SuspendMembershipCommand(
            vm.MembershipId, vm.Reason, User.GetUserId()), ct);

        if (result.IsFailure)
            return RedirectWithError(result.Error!,
                "Details", new { controller = "Trainees", id = vm.TraineeId });

        return RedirectWithSuccess("Membership suspended.",
            "Details", new { controller = "Trainees", id = vm.TraineeId });
    }

    // ── Unsuspend ──────────────────────────────────────────────────────────────
    // POST /Memberships/Unsuspend
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unsuspend(UnsuspendMembershipViewModel vm, CancellationToken ct)
    {
        var result = await Mediator.Send(new UnsuspendMembershipCommand(
            vm.MembershipId, vm.Notes ?? string.Empty, User.GetUserId()), ct);

        if (result.IsFailure)
            return RedirectWithError(result.Error!,
                "Details", new { controller = "Trainees", id = vm.TraineeId });

        return RedirectWithSuccess("Membership reactivated.",
            "Details", new { controller = "Trainees", id = vm.TraineeId });
    }
}
