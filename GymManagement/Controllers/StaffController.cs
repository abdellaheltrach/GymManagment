using GymManagement.Application.Features.Receptionists.Commands.Models;
using GymManagement.Application.Features.Receptionists.Queries.Models;
using GymManagement.Domain.Enums;
using GymManagement.Web.Bases;
using GymManagement.Web.Extensions;
using GymManagement.Web.ViewModels.Shared;
using GymManagement.Web.ViewModels.Staf;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class StaffController : BaseController
    {
        // ── List ───────────────────────────────────────────────────────────────
        // GET /Staff/Receptionists
        [HttpGet]
        public async Task<IActionResult> Receptionists(
            int page = 1,
            string? search = null,
            bool? active = null,
            CancellationToken ct = default)
        {
            var result = await Mediator.Send(
                new GetReceptionistsListQuery(page, 20, search, active), ct);

            return HandleResult(result, paged => View(new ReceptionistListViewModel
            {
                Items = paged.Items.Select(r => new ReceptionistSummaryVm
                {
                    Id = r.Id,
                    FullName = r.FullName,
                    Email = r.Email,
                    Phone = r.Phone,
                    HireDate = r.HireDate,
                    IsActive = r.IsActive,
                    Permissions = r.Permissions
                }).ToList(),
                SearchTerm = search,
                FilterActive = active,
                Pagination = new PaginationViewModel
                {
                    CurrentPage = paged.Page,
                    TotalPages = paged.TotalPages,
                    TotalCount = paged.TotalCount,
                    SearchTerm = search,
                    ActionName = nameof(Receptionists),
                    ControllerName = "Staff"
                }
            }));
        }

        // ── Details ────────────────────────────────────────────────────────────
        // GET /Staff/Details?id={guid}
        [HttpGet]
        public async Task<IActionResult> Details(Guid id, CancellationToken ct)
        {
            var result = await Mediator.Send(new GetReceptionistByIdQuery(id), ct);
            return HandleResult(result, dto => View(new ReceptionistDetailViewModel
            {
                Id = dto.Id,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Phone = dto.Phone,
                HireDate = dto.HireDate,
                IsActive = dto.IsActive,
                Permissions = dto.Permissions,
                CreatedAt = dto.CreatedAt
            }));
        }

        // ── Create ─────────────────────────────────────────────────────────────
        // GET /Staff/Create
        [HttpGet]
        public IActionResult Create()
            => View(new CreateReceptionistViewModel());

        // POST /Staff/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreateReceptionistViewModel vm, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View(vm);

            var result = await Mediator.Send(new CreateReceptionistCommand(
                vm.FirstName,
                vm.LastName,
                vm.Email,
                vm.Phone,
                vm.Password,
                vm.ComputedPermissions,
                User.GetUserId()), ct);

            if (result.IsFailure)
            {
                ModelState.AddModelError(string.Empty, result.Error!);
                return View(vm);
            }

            return RedirectWithSuccess(
                $"Receptionist {vm.FirstName} {vm.LastName} created successfully.",
                nameof(Details), new { id = result.Value });
        }

        // ── Edit ───────────────────────────────────────────────────────────────
        // GET /Staff/Edit?id={guid}
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
        {
            var result = await Mediator.Send(new GetReceptionistByIdQuery(id), ct);
            return HandleResult(result, dto =>
            {
                // Expand bitmask back to list of individual int bit values
                var selected = PermissionUiHelper.AllPermissions
                    .Select(p => (int)p.Value)
                    .Where(bit => (dto.Permissions & (ReceptionistPermission)bit) != 0)
                    .ToList();

                return View(new EditReceptionistViewModel
                {
                    ReceptionistId = dto.Id,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Phone = dto.Phone,
                    SelectedPermissions = selected
                });
            });
        }

        // POST /Staff/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            EditReceptionistViewModel vm, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View(vm);

            // Update profile
            var profileResult = await Mediator.Send(new UpdateReceptionistCommand(
                vm.ReceptionistId, vm.FirstName, vm.LastName, vm.Phone), ct);

            if (profileResult.IsFailure)
            {
                ModelState.AddModelError(string.Empty, profileResult.Error!);
                return View(vm);
            }

            // Update permissions
            var permResult = await Mediator.Send(new UpdateReceptionistPermissionsCommand(
                vm.ReceptionistId, vm.ComputedPermissions), ct);

            if (permResult.IsFailure)
            {
                ModelState.AddModelError(string.Empty, permResult.Error!);
                return View(vm);
            }

            return RedirectWithSuccess(
                "Receptionist updated. Permission changes take effect on their next request.",
                nameof(Details), new { id = vm.ReceptionistId });
        }

        // ── Deactivate ─────────────────────────────────────────────────────────
        // POST /Staff/Deactivate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
        {
            var result = await Mediator.Send(
                new DeactivateReceptionistCommand(id), ct);

            if (result.IsFailure)
                return RedirectWithError(result.Error!, nameof(Details), new { id });

            return RedirectWithSuccess(
                "Receptionist deactivated. They will be signed out on their next request.",
                nameof(Details), new { id });
        }

        // ── Reactivate ─────────────────────────────────────────────────────────
        // POST /Staff/Reactivate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reactivate(Guid id, CancellationToken ct)
        {
            var result = await Mediator.Send(
                new ReactivateReceptionistCommand(id), ct);

            if (result.IsFailure)
                return RedirectWithError(result.Error!, nameof(Details), new { id });

            return RedirectWithSuccess(
                "Receptionist reactivated. They can now log in.",
                nameof(Details), new { id });
        }
    }
}

