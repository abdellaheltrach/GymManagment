using AutoMapper;
using GymManagement.Application._Features.Trainees.Queries.Models;
using GymManagement.Application.Common.DTOs;
using GymManagement.Domain.Enums;
using GymManagement.Domain.Interfaces;
using GymManagement.Domain.Results;
using MediatR;

namespace GymManagement.Application._Features.Trainees.Queries.Handlers;

public class GetTraineeByIdQueryHandler(IUnitOfWork uow, IMapper mapper) : IRequestHandler<GetTraineeByIdQuery, Result<TraineeDetailDto>>
{

    public async Task<Result<TraineeDetailDto>> Handle(GetTraineeByIdQuery query, CancellationToken ct)
    {
        var trainee = await uow.Trainees.GetByIdAsync(query.TraineeId, ct);
        if (trainee is null)
            return Result<TraineeDetailDto>.NotFound("Trainee", query.TraineeId);

        // Active membership
        var activeMembership = (await uow.Memberships.FindAsync(
            m => m.TraineeId == trainee.Id &&
                 m.Status == MembershipStatus.Active, ct))
            .FirstOrDefault();

        var membershipDto = activeMembership is not null
            ? mapper.Map<MembershipDto>(activeMembership)
            : null;

        // Assigned trainer
        var activeAssignment = (await uow.TrainerAssignments.FindAsync(
            a => a.TraineeId == trainee.Id &&
                 a.RemovedAt == null, ct))
            .FirstOrDefault();

        string? trainerName = null;
        if (activeAssignment is not null)
        {
            var trainer = await uow.Trainers.GetByIdAsync(activeAssignment.TrainerId, ct);
            trainerName = trainer?.FullName;
        }

        // Last check-in
        var lastAttendance = (await uow.Attendances.FindAsync(
            a => a.TraineeId == trainee.Id, ct))
            .OrderByDescending(a => a.CheckInTime)
            .FirstOrDefault();

        var dto = new TraineeDetailDto(
            trainee.Id,
            trainee.FirstName,
            trainee.LastName,
            trainee.Email,
            trainee.Phone,
            trainee.Address,
            trainee.DateOfBirth,
            trainee.Age,
            trainee.Gender,
            trainee.PhotoPath,
            trainee.MedicalNotes,
            trainee.HeightCm,
            trainee.WeightKg,
            trainee.Bmi,
            trainee.BmiCategory,
            trainee.JoinDate,
            membershipDto,
            trainerName,
            lastAttendance?.CheckInTime
        );

        return Result<TraineeDetailDto>.Success(dto);
    }
}
