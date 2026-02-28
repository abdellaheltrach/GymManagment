using GymManagement.Application._Features.Trainees.Commands.Models;
using GymManagement.Application._Features.Trainees.Queries.Models;
using GymManagement.Domain.Enums;
using GymManagement.Web.Bases;
using GymManagement.Web.Extensions;
using GymManagement.Web.ViewModels.Shared;
using GymManagement.Web.ViewModels.Trainees;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Web.Controllers;


[Authorize]
public class TraineesController : BaseController
{
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