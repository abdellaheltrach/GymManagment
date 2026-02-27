using GymManagement.Application._Features.Memberships.Queries.Models;
using GymManagement.Application._Features.Payments.Commands.Models;
using GymManagement.Application._Features.Trainees.Queries.Models;
using GymManagement.Web.Bases;
using GymManagement.Web.Extensions;
using GymManagement.Web.ViewModels.Memberships;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Web.Controllers
{

    [Authorize(Policy = "CanRecordPayments")]
    [Route("[controller]")]
    public class PaymentsController : BaseController
    {
        // ── Record Payment ─────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Record(Guid traineeId, CancellationToken ct)
        {
            var traineeResult = await Mediator.Send(new GetTraineeByIdQuery(traineeId), ct);
            var membershipResult = await Mediator.Send(new GetActiveMembershipQuery(traineeId), ct);

            if (traineeResult.IsFailure) return NotFound();
            if (membershipResult.IsFailure) return RedirectWithError(
                "No active membership found.", "Details",
                new { controller = "Trainees", id = traineeId });

            var membership = membershipResult.Value!;

            return View(new RecordPaymentViewModel
            {
                MembershipId = membership.Id,
                TraineeName = traineeResult.Value!.FirstName + " " + traineeResult.Value.LastName,
                PlanName = membership.PlanName,
                TotalAmount = membership.TotalAmount,
                AmountPaid = membership.AmountPaid,
                RemainingBalance = membership.RemainingBalance,
                Amount = membership.RemainingBalance
            });
        }

        [HttpPost("record")]
        [Authorize(Policy = "CanManagePayments")]
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
                $"Payment of {vm.Amount:C} recorded.", "Index",
                new { controller = "Trainees" });
        }

        // ── Refund ─────────────────────────────────────────────────────────────────
        [HttpPost("refund")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Refund(Guid paymentId, string reason, CancellationToken ct)
        {
            var result = await Mediator.Send(
                new RefundPaymentCommand(paymentId, reason, User.GetUserId()), ct);

            if (result.IsFailure)
                return RedirectWithError(result.Error!, "Index", new { controller = "Trainees" });

            return RedirectWithSuccess("Payment refunded.", "Index", new { controller = "Trainees" });
        }
    }
}
