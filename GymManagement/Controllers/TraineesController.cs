using GymManagement.Application.Features.Trainees.Commands.Models;
using GymManagement.Application.Features.Trainees.Queries.Models;
using GymManagement.Application.Features.Memberships.Commands.PayTrainerAddon;
using GymManagement.Application.Features.Trainers.Commands.AssignOrReassignTrainer;
using GymManagement.Application.Features.Trainers.Commands.UpgradeAndAssignTrainer;
using GymManagement.Domain.Interfaces;
using GymManagement.Domain.Enums;
using GymManagement.Web.Bases;
using GymManagement.Web.Extensions;
using GymManagement.Web.ViewModels.Shared;
using GymManagement.Web.ViewModels.Trainees;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Web.Controllers;

// ── DO NOT add [Route("[controller]")] here ───────────────────────────────────
// This controller uses convention routing defined in Program.cs:
//   pattern: "{controller=Dashboard}/{action=Index}/{id?}"
// Mixing a class-level [Route] attribute with convention routing breaks all
// action resolution — MVC can no longer match GET /Trainees to Index().
[Authorize]
public class TraineesController : BaseController
{
    private readonly IUnitOfWork _uow;
    public TraineesController(IUnitOfWork uow) => _uow = uow;

    // ── List ───────────────────────────────────────────────────────────────────
    // GET /Trainees  or  GET /Trainees/Index
    [HttpGet]
    public async Task<IActionResult> Index(
        int page = 1,
        string? search = null,
        MembershipStatus? status = null,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(
            new GetTraineesListQuery(page, 20, search, status), ct);

        return HandleResult(result, paged => View(new TraineeListViewModel
        {
            Trainees = paged.Items,
            SearchTerm = search,
            StatusFilter = status,
            Pagination = new PaginationViewModel
            {
                CurrentPage = paged.Page,
                TotalPages = paged.TotalPages,
                TotalCount = paged.TotalCount,
                SearchTerm = search,
                ActionName = nameof(Index),
                ControllerName = "Trainees"
            }
        }));
    }

    // ── Details ────────────────────────────────────────────────────────────────
    // GET /Trainees/Details?id={guid}
    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetTraineeByIdQuery(id), ct);
        return HandleResult(result, dto => View(new TraineeDetailViewModel { Trainee = dto }));
    }

    // ── Create ─────────────────────────────────────────────────────────────────
    // GET /Trainees/Create
    [HttpGet]
    [Authorize(Policy = "CanManageTrainees")]
    public IActionResult Create() => View(new RegisterTraineeViewModel());

    // POST /Trainees/Create
    [HttpPost]
    [Authorize(Policy = "CanManageTrainees")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RegisterTraineeViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(vm);

        var result = await Mediator.Send(new RegisterTraineeCommand(
            vm.FirstName, vm.LastName, vm.Email, vm.Phone, vm.NationalId,
            vm.DateOfBirth, vm.Gender, vm.EmergencyContactName,
            vm.EmergencyContactPhone, vm.EmergencyContactRelation,
            vm.Address, vm.MedicalNotes, vm.HeightCm, vm.WeightKg,
            User.GetUserId()), ct);

        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(vm);
        }

        return RedirectWithSuccess(
            $"Trainee {vm.FirstName} {vm.LastName} registered successfully.",
            nameof(Details), new { id = result.Value });
    }

    // ── Edit ───────────────────────────────────────────────────────────────────
    // GET /Trainees/Edit?id={guid}
    [HttpGet]
    [Authorize(Policy = "CanManageTrainees")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetTraineeByIdQuery(id), ct);
        return HandleResult(result, dto => View(new EditTraineeViewModel
        {
            TraineeId = dto.Id,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            NationalId = string.Empty,
            DateOfBirth = dto.DateOfBirth,
            Gender = dto.Gender,
            Address = dto.Address,
            MedicalNotes = dto.MedicalNotes,
            HeightCm = dto.HeightCm,
            WeightKg = dto.WeightKg,
            // Pre-populate emergency contact from trainee data
            EmergencyContactName = dto.LastName,
            EmergencyContactPhone = dto.Phone,
        }));
    }

    // POST /Trainees/Edit
    [HttpPost]
    [Authorize(Policy = "CanManageTrainees")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditTraineeViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(vm);

        var result = await Mediator.Send(new UpdateTraineeCommand(
            vm.TraineeId, vm.FirstName, vm.LastName, vm.Phone,
            vm.Address, vm.MedicalNotes, vm.HeightCm, vm.WeightKg,
            vm.EmergencyContactName ?? string.Empty,
            vm.EmergencyContactPhone ?? string.Empty,
            vm.EmergencyContactRelation, User.GetUserId()), ct);

        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(vm);
        }

        return RedirectWithSuccess("Trainee updated.", nameof(Details), new { id = vm.TraineeId });
    }

    // ── Delete ─────────────────────────────────────────────────────────────────
    // POST /Trainees/Delete
    [HttpPost]
    [Authorize(Policy = "CanManageTrainees")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new DeleteTraineeCommand(id, User.GetUserId()), ct);

        if (result.IsFailure)
            return RedirectWithError(result.Error!, nameof(Details), new { id });

        return RedirectWithSuccess("Trainee deleted.", nameof(Index));
    }

    // ── Assign / Reassign Trainer ────────────────────────────────────────────
    // GET /Trainees/AssignTrainer?traineeId={guid}
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignTrainer(Guid traineeId, CancellationToken ct)
    {
        var trainee = await _uow.Trainees.GetByIdAsync(traineeId, ct);
        if (trainee is null) return NotFound();

        // Only load active trainers for the dropdown
        var trainers = await _uow.Trainers.FindAsync(
            t => t.IsActive, ct);

        // Load current assignment if any
        var current = await _uow.TrainerAssignments.FirstOrDefaultAsync(
            a => a.TraineeId == traineeId && a.RemovedAt == null, ct);

        // Check if trainee has a qualifying membership
        var activeMembership = (await _uow.Memberships.FindAsync(
            m => m.TraineeId == traineeId &&
                 m.Status == GymManagement.Domain.Enums.MembershipStatus.Active &&
                 m.EndDate > DateTime.UtcNow, ct)).FirstOrDefault();

        bool planIncludesTrainer = false;
        string planName = "None";
        bool trainerAddonPaid = false;
        decimal trainerAddonFee = 0;

        if (activeMembership is not null)
        {
            var plan = await _uow.MembershipPlans.GetByIdAsync(activeMembership.PlanId, ct);
            planIncludesTrainer = plan?.IncludesPersonalTrainer ?? false;
            planName            = plan?.Name ?? "Unknown";
            trainerAddonFee     = plan?.TrainerAddonFee ?? 0;
            trainerAddonPaid    = activeMembership.TrainerAddonPaid;
        }

        // If plan does not include trainer, get eligible plans for upgrade dropdown
        List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> eligiblePlans = [];
        if (!planIncludesTrainer && activeMembership is not null)
        {
            eligiblePlans = (await _uow.MembershipPlans.FindAsync(p => p.IncludesPersonalTrainer && p.IsActive, ct))
                .OrderBy(p => p.Price)
                .Select(p => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(
                    $"{p.Name} — {p.Price:C} (+ {p.TrainerAddonFee:C} Add-on)",
                    p.Id.ToString()))
                .ToList();
        }

        return View(new AssignTrainerViewModel
        {
            TraineeId = traineeId,
            TraineeName = trainee.FullName,
            MembershipId        = activeMembership?.Id,
            CurrentTrainerName = current is not null
                ? (await _uow.Trainers.GetByIdAsync(current.TrainerId, ct))?.FullName
                : null,
            PlanName = planName,
            PlanIncludesTrainer = planIncludesTrainer,
            HasActiveMembership = activeMembership is not null,
            TrainerAddonPaid    = trainerAddonPaid,
            TrainerAddonFee     = trainerAddonFee,
            EligiblePlans       = eligiblePlans,
            AvailableTrainers = trainers
                .OrderBy(t => t.FullName)
                .Select(t => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(
                    $"{t.FullName} — {t.Specialization}",
                    t.Id.ToString()))
                .ToList()
        });
    }

    // POST /Trainees/AssignTrainer
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignTrainer(
        AssignTrainerViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var result = await Mediator.Send(
            new AssignOrReassignTrainerCommand(
                vm.TraineeId, vm.SelectedTrainerId, User.GetUserId()), ct);

        if (result.IsFailure)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Details), new { id = vm.TraineeId });
        }

        return RedirectWithSuccess(
            "Trainer assigned successfully.",
            nameof(Details), new { id = vm.TraineeId });
    }

    // POST /Trainees/PayTrainerAddon
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PayTrainerAddon(
        PayTrainerAddonViewModel vm, CancellationToken ct)
    {
        var result = await Mediator.Send(new PayTrainerAddonCommand(
            vm.TraineeId,
            vm.Amount,
            vm.Method,
            User.GetUserId(),
            vm.Notes), ct);

        if (result.IsFailure)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(AssignTrainer), new { traineeId = vm.TraineeId });
        }

        return RedirectWithSuccess(
            "Trainer add-on payment recorded. You can now assign a trainer.",
            nameof(AssignTrainer), new { traineeId = vm.TraineeId });
    }

    // POST /Trainees/UpgradeAndAssignTrainer
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpgradeAndAssignTrainer(
        AssignTrainerViewModel vm, CancellationToken ct)
    {
        if (vm.NewPlanId == null || vm.SelectedTrainerId == Guid.Empty)
        {
            TempData["Error"] = "Please select both a plan and a trainer.";
            return RedirectToAction(nameof(AssignTrainer), new { traineeId = vm.TraineeId });
        }

        var result = await Mediator.Send(new UpgradeAndAssignTrainerCommand(
            vm.TraineeId,
            vm.NewPlanId.Value,
            vm.SelectedTrainerId,
            vm.UpgradePaymentMethod,
            User.GetUserId()), ct);

        if (result.IsFailure)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(AssignTrainer), new { traineeId = vm.TraineeId });
        }

        return RedirectWithSuccess(
            "Membership upgraded and trainer assigned successfully.",
            nameof(Details), new { id = vm.TraineeId });
    }

    // POST /Trainees/RemoveTrainer
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveTrainer(Guid traineeId, CancellationToken ct)
    {
        var assignment = await _uow.TrainerAssignments.FirstOrDefaultAsync(
            a => a.TraineeId == traineeId && a.RemovedAt == null, ct);

        if (assignment is null)
            return RedirectWithError(
                "No active trainer assignment found.",
                nameof(Details), new { id = traineeId });

        assignment.RemovedAt = DateTime.UtcNow;
        assignment.RemovedById = User.GetUserId();
        assignment.RemovalReason = "Removed by admin";
        _uow.TrainerAssignments.Update(assignment);
        await _uow.SaveChangesAsync(ct);

        return RedirectWithSuccess(
            "Trainer removed.", nameof(Details), new { id = traineeId });
    }

    // ── Anonymise ──────────────────────────────────────────────────────────────
    // POST /Trainees/Anonymise
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Anonymise(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new AnonymiseTraineeCommand(id, User.GetUserId()), ct);

        if (result.IsFailure)
            return RedirectWithError(result.Error!, nameof(Details), new { id });

        return RedirectWithSuccess("Trainee data anonymised (GDPR).", nameof(Index));
    }
}