using GymManagement.Application.Features.Receptionists.Commands.Models;
using GymManagement.Domain.Entities;
using GymManagement.Domain.Enums;
using GymManagement.Domain.Interfaces;
using GymManagement.Domain.Results;
using MediatR;

namespace GymManagement.Application.Features.Receptionists.Commands.Handlers
{
    public class CreateReceptionistCommandHandler(
        IUnitOfWork uow,
        IIdentityService identityService) : IRequestHandler<CreateReceptionistCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(CreateReceptionistCommand cmd, CancellationToken ct)
        {
            // ── Guards ──────────────────────────────────────────────────────────
            if (await identityService.UserExistsAsync(cmd.Email))
                return Result<Guid>.Conflict($"A user with email '{cmd.Email}' already exists.");

            // ── Phase 1: Identity ───────────────────────────────────────────────
            var userResult = await identityService.CreateUserAsync(
                cmd.Email, cmd.Password, cmd.FirstName, cmd.LastName);

            if (userResult.IsFailure)
                return Result<Guid>.Failure(userResult.Error!);

            var userId = userResult.Value!;
            await uow.SaveChangesAsync(ct);

            var roleResult = await identityService.AddToRoleAsync(userId, "Receptionist");
            if (roleResult.IsFailure)
                return Result<Guid>.Failure(roleResult.Error!);

            // ── Phase 2: Domain writes ──────────────────────────────────────────
            var receptionist = new Receptionist
            {
                ApplicationUserId = userId,
                FirstName = cmd.FirstName,
                LastName = cmd.LastName,
                Email = cmd.Email,
                Phone = cmd.Phone,
                HireDate = DateTime.UtcNow,
                IsActive = true,
                Permissions = cmd.Permissions,
                CreatedById = cmd.CreatedById
            };

            await uow.Receptionists.AddAsync(receptionist, ct);

            await uow.Notifications.AddAsync(new Notification
            {
                UserId = userId,
                Title = "Welcome to the Team!",
                Body = $"Welcome {receptionist.FirstName}! Your receptionist account has been created.",
                Type = NotificationType.General
            }, ct);

            return Result<Guid>.Success(receptionist.Id);
        }
    }
}
