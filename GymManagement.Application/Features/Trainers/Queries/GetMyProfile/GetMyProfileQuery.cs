using GymManagement.Application.Common.Behaviours;
using GymManagement.Domain.Interfaces;
using GymManagement.Domain.Results;
using MediatR;

namespace GymManagement.Application.Features.Trainers.Queries.GetMyProfile;

public record GetMyProfileQuery(string ApplicationUserId)
    : IQuery<TrainerProfileDto>;

public record TrainerProfileDto(
    Guid Id,
    string FullName,
    string Email,
    string Phone,
    string Specialization,
    string? Bio,
    int YearsOfExperience,
    string SalaryType,
    decimal? BaseSalary,
    decimal? CommissionPerTrainee,
    DateTime HireDate,
    int AssignedTrainees,
    int ActivePrograms,
    int SessionsThisMonth
);

public class GetMyProfileHandler
    : IRequestHandler<GetMyProfileQuery, Result<TrainerProfileDto>>
{
    private readonly IUnitOfWork _uow;
    public GetMyProfileHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<TrainerProfileDto>> Handle(
        GetMyProfileQuery query, CancellationToken ct)
    {
        var trainers = await _uow.Trainers.FindAsync(
            t => t.ApplicationUserId == query.ApplicationUserId, ct);

        var trainer = trainers.FirstOrDefault();
        if (trainer is null)
            return Result<TrainerProfileDto>.NotFound(
                "Trainer", Guid.Parse(query.ApplicationUserId));

        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var assignments = await _uow.TrainerAssignments.FindAsync(
            a => a.TrainerId == trainer.Id && a.RemovedAt == null, ct);

        var activePrograms = await _uow.TrainingPrograms.CountAsync(
            p => p.TrainerId == trainer.Id && p.IsActive, ct);

        var traineeIds = assignments.Select(a => a.TraineeId).ToList();

        var sessionsThisMonth = await _uow.Attendances.CountAsync(
            a => traineeIds.Contains(a.TraineeId) &&
                 a.CheckInTime >= monthStart, ct);

        return Result<TrainerProfileDto>.Success(new TrainerProfileDto(
            trainer.Id,
            trainer.FullName,
            trainer.Email,
            trainer.Phone,
            trainer.Specialization.ToString(),
            trainer.Bio,
            trainer.YearsOfExperience,
            trainer.SalaryType.ToString(),
            trainer.BaseSalary,
            trainer.CommissionPerTrainee,
            trainer.HireDate,
            assignments.Count,
            activePrograms,
            sessionsThisMonth
        ));
    }
}
