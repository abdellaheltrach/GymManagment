using GymManagement.Domain.Entities;
using GymManagement.Domain.Interfaces;
using GymManagement.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace GymManagement.Infrastructure.Bases
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction? _transaction;

        // ── Lazy-initialised repositories ──────────────────────────────────────
        private IRepository<Trainee>? _trainees;
        private IRepository<Trainer>? _trainers;
        private IRepository<MembershipPlan>? _membershipPlans;
        private IRepository<Membership>? _memberships;
        private IRepository<Payment>? _payments;
        private IRepository<FrozenPeriod>? _frozenPeriods;
        private IRepository<TrainerAssignment>? _trainerAssignments;
        private IRepository<Attendance>? _attendances;
        private IRepository<TrainingProgram>? _trainingPrograms;
        private IRepository<Exercise>? _exercises;
        private IRepository<ProgressRecord>? _progressRecords;
        private IRepository<Notification>? _notifications;
        private IRepository<AuditLog>? _auditLogs;

        public UnitOfWork(AppDbContext context) => _context = context;

        public IRepository<Trainee> Trainees => _trainees ??= new Repository<Trainee>(_context);
        public IRepository<Trainer> Trainers => _trainers ??= new Repository<Trainer>(_context);
        public IRepository<MembershipPlan> MembershipPlans => _membershipPlans ??= new Repository<MembershipPlan>(_context);
        public IRepository<Membership> Memberships => _memberships ??= new Repository<Membership>(_context);
        public IRepository<Payment> Payments => _payments ??= new Repository<Payment>(_context);
        public IRepository<FrozenPeriod> FrozenPeriods => _frozenPeriods ??= new Repository<FrozenPeriod>(_context);
        public IRepository<TrainerAssignment> TrainerAssignments => _trainerAssignments ??= new Repository<TrainerAssignment>(_context);
        public IRepository<Attendance> Attendances => _attendances ??= new Repository<Attendance>(_context);
        public IRepository<TrainingProgram> TrainingPrograms => _trainingPrograms ??= new Repository<TrainingProgram>(_context);
        public IRepository<Exercise> Exercises => _exercises ??= new Repository<Exercise>(_context);
        public IRepository<ProgressRecord> ProgressRecords => _progressRecords ??= new Repository<ProgressRecord>(_context);
        public IRepository<Notification> Notifications => _notifications ??= new Repository<Notification>(_context);
        public IRepository<AuditLog> AuditLogs => _auditLogs ??= new Repository<AuditLog>(_context);

        // ── Persistence ────────────────────────────────────────────────────────
        public async Task<int> SaveChangesAsync(CancellationToken ct = default)
            => await _context.SaveChangesAsync(ct);

        public async Task ExecuteInTransactionAsync(
            Func<CancellationToken, Task> operation,
            CancellationToken ct = default)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async cancellationToken =>
            {
                await using var tx =
                    await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    await operation(cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);
                    await tx.CommitAsync(cancellationToken);
                }
                catch
                {
                    await tx.RollbackAsync(cancellationToken);
                    throw;
                }
            }, ct);
        }

        // ── Transaction management ─────────────────────────────────────────────
        public async Task BeginTransactionAsync(CancellationToken ct = default)
            => _transaction = await _context.Database.BeginTransactionAsync(ct);

        public async Task CommitTransactionAsync(CancellationToken ct = default)
        {
            if (_transaction is null)
                throw new InvalidOperationException("No active transaction to commit.");

            await _transaction.CommitAsync(ct);
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        public async Task RollbackTransactionAsync(CancellationToken ct = default)
        {
            if (_transaction is null) return;

            await _transaction.RollbackAsync(ct);
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
