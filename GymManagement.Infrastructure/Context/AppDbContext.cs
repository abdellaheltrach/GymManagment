using GymManagement.Domain.Bases;
using GymManagement.Domain.Entities;
using GymManagement.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Reflection;

namespace GymManagement.Infrastructure.Context
{

    /// <summary>
    /// Single DbContext for the entire application.
    /// Inherits IdentityDbContext to combine Identity tables with domain tables in one DB.
    /// Global query filters for soft delete are registered here — never per-query.
    /// SaveChangesAsync is overridden to:
    ///   1. Stamp UpdatedAt on modified entities
    ///   2. Handle soft delete (flip IsDeleted instead of DELETE)
    /// The AuditInterceptor is injected via AddInterceptors() in DI — not inline here.
    /// </summary>
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected AppDbContext()
        {
        }

        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Trainee> Trainees => Set<Trainee>();
        public DbSet<Trainer> Trainers => Set<Trainer>();
        public DbSet<MembershipPlan> MembershipPlans => Set<MembershipPlan>();
        public DbSet<Membership> Memberships => Set<Membership>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<FrozenPeriod> FrozenPeriods => Set<FrozenPeriod>();
        public DbSet<TrainerAssignment> TrainerAssignments => Set<TrainerAssignment>();
        public DbSet<Attendance> Attendances => Set<Attendance>();
        public DbSet<TrainingProgram> TrainingPrograms => Set<TrainingProgram>();
        public DbSet<Exercise> Exercises => Set<Exercise>();
        public DbSet<ProgressRecord> ProgressRecords => Set<ProgressRecord>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // Identity tables first

            // Apply all IEntityTypeConfiguration<T> classes from this assembly
            builder.ApplyConfigurationsFromAssembly(
                Assembly.GetAssembly(typeof(AppDbContext))!);

            // ── Global query filters — soft delete ─────────────────────────────
            // Applied here in addition to individual configurations as a safety net.
            // Any new SoftDeletableEntity added to the model is covered automatically.
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                if (typeof(SoftDeletableEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var method = typeof(AppDbContext)
                        .GetMethod(nameof(ApplySoftDeleteFilter),
                                   BindingFlags.NonPublic | BindingFlags.Static)!
                        .MakeGenericMethod(entityType.ClrType);

                    method.Invoke(null, [builder]);
                }
            }
        }

        private static void ApplySoftDeleteFilter<TEntity>(ModelBuilder builder)
            where TEntity : SoftDeletableEntity
        {
            builder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted);
        }

        // ── SaveChangesAsync override ──────────────────────────────────────────
        public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            ProcessChanges();
            return await base.SaveChangesAsync(ct);
        }

        public override int SaveChanges()
        {
            ProcessChanges();
            return base.SaveChanges();
        }

        private void ProcessChanges()
        {
            var now = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        // CreatedAt is set by the entity itself (init property)
                        // CreatedById is set by the command handler before Add()
                        break;

                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = now;
                        break;
                }
            }

            // Intercept hard deletes on SoftDeletableEntity — convert to soft delete
            foreach (var entry in ChangeTracker.Entries<SoftDeletableEntity>()
                         .Where(e => e.State == EntityState.Deleted))
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedAt = now;
                // DeletedById must be set by the caller before Remove() is called
            }
        }
    }
}
