using GymManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace GymManagement.Infrastructure.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Trainee> Trainees { get; set; }
        public DbSet<Trainer> Trainers { get; set; }
        public DbSet<Membership> Memberships { get; set; }
        public DbSet<MembershipPlan> MembershipPlans { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Exercise> Exercises { get; set; }
        public DbSet<FrozenPeriod> FrozenPeriods { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<ProgressRecord> ProgressRecords { get; set; }
        public DbSet<TrainerAssignment> TrainerAssignments { get; set; }
        public DbSet<TrainingProgram> TrainingPrograms { get; set; }

        override protected void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);
        }


    }
}
