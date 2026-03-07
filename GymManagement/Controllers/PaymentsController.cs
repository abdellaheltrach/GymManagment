
using GymManagement.Application.Features.Memberships.Queries.Models;
using GymManagement.Application.Features.Payments.Commands.Models;
using GymManagement.Application.Features.Payments.Queries.Models;
using GymManagement.Application.Features.Trainees.Queries.Models;
using GymManagement.Domain.Enums;
using GymManagement.Web.Bases;
using GymManagement.Web.Extensions;
using GymManagement.Web.ViewModels.Memberships;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Web.Controllers;

[Authorize]
public class PaymentsController : BaseController
{
    // ── Index ──────────────────────────────────────────────────────────────────
    // GET /Payments  — lists all payment records with filters
    [HttpGet]
    public async Task<IActionResult> Index(
        int page = 1,
        string? searchTerm = null,
        PaymentMethod? method = null,
        PaymentStatus? status = null,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetPaymentsListQuery(page, 20, searchTerm, method, status), ct);

        ViewBag.CurrentSearch = searchTerm;
        ViewBag.CurrentMethod = method;
        ViewBag.CurrentStatus = status;

        return HandleResult(result, paged => View(paged));
    }

    // ── Update GET ─────────────────────────────────────────────────────────────
    [HttpGet]
    [Authorize(Policy = "CanRecordPayments")]
    public async Task<IActionResult> Update(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetPaymentByIdQuery(id), ct);

        return HandleResult(result, p => View(new UpdatePaymentViewModel
        {
            PaymentId = p.Id,
            TraineeName = p.TraineeName,
            PlanName = p.PlanName,
            TotalAmount = p.Amount + p.RemainingBalance, // Total amount is Paid + Balance
            AlreadyPaid = p.Amount,
            RemainingBalance = p.RemainingBalance,
            Method = p.Method,
            AdditionalAmount = p.RemainingBalance
        }));
    }

    // ── Update POST ────────────────────────────────────────────────────────────
    [HttpPost]
    [Authorize(Policy = "CanRecordPayments")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(UpdatePaymentViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(vm);

        var result = await Mediator.Send(new UpdatePaymentCommand(
            vm.PaymentId, vm.AdditionalAmount, vm.Method,
            User.GetUserId(), vm.Notes), ct);

        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(vm);
        }

        return RedirectWithSuccess(
            $"Payment updated. Additional {vm.AdditionalAmount:C} recorded.",
            "Index");
    }

    // ── Record GET ─────────────────────────────────────────────────────────────
    // GET /Payments/Record?traineeId={guid}
    [HttpGet]
    [Authorize(Policy = "CanRecordPayments")]
    public async Task<IActionResult> Record(Guid traineeId, CancellationToken ct)
    {
        var traineeResult = await Mediator.Send(new GetTraineeByIdQuery(traineeId), ct);
        var membershipResult = await Mediator.Send(new GetActiveMembershipQuery(traineeId), ct);

        if (traineeResult.IsFailure) return NotFound();

        if (membershipResult.IsFailure)
            return RedirectWithError(
                "No active membership found for this trainee.",
                "Details", new { controller = "Trainees", id = traineeId });

        var membership = membershipResult.Value!;

        return View(new RecordPaymentViewModel
        {
            MembershipId = membership.Id,
            TraineeName = traineeResult.Value!.FirstName + " " + traineeResult.Value.LastName,
            PlanName = membership.PlanName,
            TotalAmount = membership.TotalAmount,
            AmountPaid = membership.AmountPaid,
            RemainingBalance = membership.RemainingBalance,
            Amount = membership.RemainingBalance  // pre-fill with full balance due
        });
    }

    // ── Record POST ────────────────────────────────────────────────────────────
    // POST /Payments/Record
    [HttpPost]
    [Authorize(Policy = "CanRecordPayments")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Record(RecordPaymentViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(vm);

        var result = await Mediator.Send(new RecordPaymentCommand(
            vm.MembershipId, vm.Amount, vm.Method,
            User.GetUserId(), vm.Notes), ct);

        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(vm);
        }

        return RedirectWithSuccess(
            $"Payment of {vm.Amount:C} recorded successfully.",
            "Index", new { controller = "Trainees" });
    }

    // ── Refund POST ────────────────────────────────────────────────────────────
    // POST /Payments/Refund
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Refund(Guid paymentId, string reason, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new RefundPaymentCommand(paymentId, reason, User.GetUserId()), ct);

        if (result.IsFailure)
            return RedirectWithError(result.Error!, "Index");

        return RedirectWithSuccess("Payment refunded.", "Index");
    }
}