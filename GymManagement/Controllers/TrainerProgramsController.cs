using GymManagement.Application.Features.TrainingPrograms.Commands.AddExercise;
using GymManagement.Application.Features.TrainingPrograms.Commands.CreateTrainingProgram;
using GymManagement.Application.Features.TrainingPrograms.Queries.GetTrainerPrograms;
using GymManagement.Domain.Interfaces;
using GymManagement.Web.Bases;
using GymManagement.Web.ViewModels.TrainerPrograms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GymManagement.Web.Controllers;

[Authorize(Roles = "Trainer")]
public class TrainerProgramsController : BaseController
{
    private readonly IUnitOfWork _uow;
    public TrainerProgramsController(IUnitOfWork uow) => _uow = uow;

    // GET /TrainerPrograms
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var trainerId = await ResolveTrainerIdAsync(ct);
        if (trainerId is null) return Forbid();

        var result = await Mediator.Send(
            new GetTrainerProgramsQuery(trainerId.Value), ct);

        return HandleResult(result, programs => View(
            new TrainerProgramsListViewModel { Programs = programs }));
    }

    // GET /TrainerPrograms/Create
    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        var trainerId = await ResolveTrainerIdAsync(ct);
        if (trainerId is null) return Forbid();

        var vm = new CreateProgramViewModel
        {
            StartDate = DateTime.Today,
            TraineeList = await BuildTraineeSelectListAsync(trainerId.Value, ct)
        };
        return View(vm);
    }

    // POST /TrainerPrograms/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateProgramViewModel vm, CancellationToken ct)
    {
        var trainerId = await ResolveTrainerIdAsync(ct);
        if (trainerId is null) return Forbid();

        if (!ModelState.IsValid)
        {
            vm.TraineeList = await BuildTraineeSelectListAsync(trainerId.Value, ct);
            return View(vm);
        }

        var result = await Mediator.Send(new CreateTrainingProgramCommand(
            trainerId.Value, vm.TraineeId, vm.Title,
            vm.Description, vm.StartDate, vm.EndDate), ct);

        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            vm.TraineeList = await BuildTraineeSelectListAsync(trainerId.Value, ct);
            return View(vm);
        }

        return RedirectWithSuccess(
            "Training program created.",
            "Details", new { id = result.Value });
    }

    // GET /TrainerPrograms/Details?id={guid}
    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken ct)
    {
        var trainerId = await ResolveTrainerIdAsync(ct);
        if (trainerId is null) return Forbid();

        var program = await _uow.TrainingPrograms.GetByIdAsync(id, ct);
        if (program is null) return NotFound();
        if (program.TrainerId != trainerId.Value) return Forbid();

        var result = await Mediator.Send(
            new GetTrainerProgramsQuery(trainerId.Value), ct);

        var dto = result.Value!.FirstOrDefault(p => p.Id == id);
        if (dto is null) return NotFound();

        return View(new ProgramDetailViewModel
        {
            Program = dto,
            AddExerciseForm = new AddExerciseViewModel { ProgramId = id }
        });
    }

    // POST /TrainerPrograms/AddExercise
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddExercise(
        AddExerciseViewModel vm, CancellationToken ct)
    {
        var trainerId = await ResolveTrainerIdAsync(ct);
        if (trainerId is null) return Forbid();

        var result = await Mediator.Send(new AddExerciseCommand(
            vm.ProgramId, trainerId.Value,
            vm.Name, vm.Sets, vm.Reps,
            vm.WeightKg, vm.DurationSeconds,
            vm.RestSeconds, vm.Notes), ct);

        if (result.IsFailure)
            return RedirectWithError(result.Error!,
                "Details", new { id = vm.ProgramId });

        return RedirectWithSuccess(
            "Exercise added.",
            "Details", new { id = vm.ProgramId });
    }

    // POST /TrainerPrograms/Deactivate
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        var trainerId = await ResolveTrainerIdAsync(ct);
        if (trainerId is null) return Forbid();

        var program = await _uow.TrainingPrograms.GetByIdAsync(id, ct);
        if (program is null) return NotFound();
        if (program.TrainerId != trainerId.Value) return Forbid();

        program.IsActive = false;
        _uow.TrainingPrograms.Update(program);
        await _uow.SaveChangesAsync(ct);

        return RedirectWithSuccess(
            "Program deactivated.", nameof(Index));
    }

    private async Task<List<SelectListItem>> BuildTraineeSelectListAsync(
        Guid trainerId, CancellationToken ct)
    {
        var assignments = await _uow.TrainerAssignments.FindAsync(
            a => a.TrainerId == trainerId && a.RemovedAt == null, ct);

        var items = new List<SelectListItem>();
        foreach (var a in assignments)
        {
            var t = await _uow.Trainees.GetByIdAsync(a.TraineeId, ct);
            if (t is not null)
                items.Add(new SelectListItem(t.FullName, t.Id.ToString()));
        }
        return items;
    }

    private async Task<Guid?> ResolveTrainerIdAsync(CancellationToken ct)
    {
        var userId = User.FindFirst(
            System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return null;
        var trainers = await _uow.Trainers.FindAsync(
            t => t.ApplicationUserId == userId, ct);
        return trainers.FirstOrDefault()?.Id;
    }
}
