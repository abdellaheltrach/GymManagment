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
            // Guard: unique email (both in domain and identity)
            if (await identityService.UserExistsAsync(cmd.Email))
                return Result<Guid>.Conflict($"A user with email '{cmd.Email}' already exists.");

            if (await uow.Trainers.AnyAsync(t => t.Email == cmd.Email, ct))
                return Result<Guid>.Conflict($"A trainer with email '{cmd.Email}' already exists.");

            await uow.BeginTransactionAsync(ct);
            try
            {
                // 1. Create Identity User
                var userResult = await identityService.CreateUserAsync(
                    cmd.Email, cmd.Password, cmd.FirstName, cmd.LastName);

                if (userResult.IsFailure)
                {
                    await uow.RollbackTransactionAsync(ct);
                    return Result<Guid>.Failure(userResult.Error!);
                }

                var userId = userResult.Value!;

                // 2. Assign Trainer Role
                var roleResult = await identityService.AddToRoleAsync(userId, "Trainer");
                if (roleResult.IsFailure)
                {
                    await uow.RollbackTransactionAsync(ct);
                    return Result<Guid>.Failure(roleResult.Error!);
                }

                // 3. Create Trainer Entity
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
                await uow.SaveChangesAsync(ct);

                // 4. Send Notification
                var notification = new Notification
                {
                    UserId = userId,
                    Title = "Welcome to the Team!",
                    Body = $"Welcome {trainer.FirstName}! Your trainer account has been created successfully.",
                    Type = NotificationType.General
                };
                await uow.Notifications.AddAsync(notification, ct);
                await uow.SaveChangesAsync(ct);

                await uow.CommitTransactionAsync(ct);

                return Result<Guid>.Success(trainer.Id);
            }
            catch (Exception ex)
            {
                await uow.RollbackTransactionAsync(ct);
                return Result<Guid>.Failure($"Failed to register trainer: {ex.Message}");
            }
        }
    }
}
