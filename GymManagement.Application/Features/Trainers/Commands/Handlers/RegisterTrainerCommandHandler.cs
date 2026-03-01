using GymManagement.Application.Features.Trainers.Commands.Models;
using GymManagement.Domain.Entities;
using GymManagement.Domain.Enums;
using GymManagement.Domain.Interfaces;
using GymManagement.Domain.Results;
using MediatR;

namespace GymManagement.Application.Features.Trainers.Commands.Handlers
{
    public class RegisterTrainerCommandHandler(
        IUnitOfWork uow,
        IIdentityService identityService) : IRequestHandler<RegisterTrainerCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(RegisterTrainerCommand cmd, CancellationToken ct)
        {
            // ── Guards ─────────────────────────────────────────────────────────────────
            if (await identityService.UserExistsAsync(cmd.Email))
                return Result<Guid>.Conflict($"A user with email '{cmd.Email}' already exists.");

            if (await uow.Trainers.AnyAsync(t => t.Email == cmd.Email, ct))
                return Result<Guid>.Conflict($"A trainer with email '{cmd.Email}' already exists.");

            // ── Phase 1: Identity (auto-committed by UserManager) ─────────────────────
            var userResult = await identityService.CreateUserAsync(
                cmd.Email, cmd.Password, cmd.FirstName, cmd.LastName);

            if (userResult.IsFailure)
                return Result<Guid>.Failure(userResult.Error!);

            var userId = userResult.Value!;
            await uow.SaveChangesAsync(ct); // Flush user so Role assignment succeeds

            var roleResult = await identityService.AddToRoleAsync(userId, "Trainer");

            if (roleResult.IsFailure)
                return Result<Guid>.Failure(roleResult.Error!);

            // ── Phase 2: Domain writes ───────────────────────────────────────────────
            var trainer = new Trainer
            {
                ApplicationUserId = userId,
                FirstName = cmd.FirstName,
                LastName = cmd.LastName,
                Email = cmd.Email,
                Phone = cmd.Phone,
                DateOfBirth = cmd.DateOfBirth,
                Specialization = cmd.Specialization,
                Bio = cmd.Bio,
                YearsOfExperience = cmd.YearsOfExperience,
                HireDate = DateTime.UtcNow,
                IsActive = true,
                SalaryType = cmd.SalaryType,
                BaseSalary = cmd.BaseSalary,
                CommissionPerTrainee = cmd.CommissionPerTrainee,
                CreatedById = cmd.CreatedById
            };

            await uow.Trainers.AddAsync(trainer, ct);

            await uow.Notifications.AddAsync(new Notification
            {
                UserId = userId,
                Title = "Welcome to the Team!",
                Body = $"Welcome {trainer.FirstName}! Your trainer account has been created successfully.",
                Type = NotificationType.General
            }, ct);

            return Result<Guid>.Success(trainer.Id);
        }

    }
}


