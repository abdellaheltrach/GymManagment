using GymManagement.Application.Features.Trainees.Commands.Models;
using GymManagement.Domain.Entities;
using GymManagement.Domain.Enums;
using GymManagement.Domain.Interfaces;
using GymManagement.Domain.Results;
using MediatR;

namespace GymManagement.Application.Features.Trainees.Commands.Handlers;

public class RegisterTraineeCommandHandler(IUnitOfWork uow) : IRequestHandler<RegisterTraineeCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(RegisterTraineeCommand cmd, CancellationToken ct)
    {
        // Guard: unique email
        if (await uow.Trainees.AnyAsync(t => t.Email == cmd.Email, ct))
            return Result<Guid>.Conflict($"A trainee with email '{cmd.Email}' already exists.");

        // Guard: unique national ID
        if (await uow.Trainees.AnyAsync(t => t.NationalId == cmd.NationalId, ct))
            return Result<Guid>.Conflict($"A trainee with national ID '{cmd.NationalId}' already exists.");

        var trainee = new Trainee
        {
            FirstName = cmd.FirstName,
            LastName = cmd.LastName,
            Email = cmd.Email,
            Phone = cmd.Phone,
            NationalId = cmd.NationalId,
            DateOfBirth = cmd.DateOfBirth,
            Gender = cmd.Gender,
            EmergencyContactName = cmd.EmergencyContactName,
            EmergencyContactPhone = cmd.EmergencyContactPhone,
            EmergencyContactRelation = cmd.EmergencyContactRelation,
            Address = cmd.Address,
            MedicalNotes = cmd.MedicalNotes,
            HeightCm = cmd.HeightCm,
            WeightKg = cmd.WeightKg,
            JoinDate = DateTime.UtcNow,
            CreatedById = cmd.CreatedById
        };

        await uow.Trainees.AddAsync(trainee, ct);

        // Welcome notification — atomic with trainee creation
        if (!string.IsNullOrEmpty(cmd.CreatedById))
        {
            var notification = new Notification
            {
                UserId = trainee.ApplicationUserId ?? cmd.CreatedById,
                Title = "Welcome to the Gym!",
                Body = $"Welcome {trainee.FirstName}! Your account has been created. Please visit reception to activate your membership.",
                Type = NotificationType.General
            };
            await uow.Notifications.AddAsync(notification, ct);
        }

        await uow.SaveChangesAsync(ct);
        return Result<Guid>.Success(trainee.Id);
    }
}

