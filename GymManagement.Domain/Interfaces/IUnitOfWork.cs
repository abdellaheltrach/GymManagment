using GymManagement.Domain.Entities;

namespace GymManagement.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    // ── Repositories ──────────────────────────────────────
    IRepository<Trainee> Trainees { get; }
    IRepository<Trainer> Trainers { get; }
    IRepository<MembershipPlan> MembershipPlans { get; }
    IRepository<Membership> Memberships { get; }
    IRepository<Payment> Payments { get; }
    IRepository<FrozenPeriod> FrozenPeriods { get; }
    IRepository<TrainerAssignment> TrainerAssignments { get; }
    IRepository<Attendance> Attendances { get; }
    IRepository<TrainingProgram> TrainingPrograms { get; }
    IRepository<Exercise> Exercises { get; }
    IRepository<ProgressRecord> ProgressRecords { get; }
    IRepository<Notification> Notifications { get; }
    IRepository<AuditLog> AuditLogs { get; }

    // ── Persistence ────────────────────────────────────────────────────────
    public Task<int> SaveChangesAsync(CancellationToken ct = default);

    Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken ct = default);

    // ── Transaction management ─────────────────────────────────────────────
    public Task BeginTransactionAsync(CancellationToken ct = default);

    public Task CommitTransactionAsync(CancellationToken ct = default);

    public Task RollbackTransactionAsync(CancellationToken ct = default);

}
