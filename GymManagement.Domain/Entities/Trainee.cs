using GymManagement.Domain.Bases;
using GymManagement.Domain.Enums;

namespace GymManagement.Domain.Entities
{

    public class Trainee : AuditableEntity
    {
        public string? ApplicationUserId { get; set; }

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";

        public DateTime DateOfBirth { get; set; }
        public int Age => (int)((DateTime.UtcNow - DateOfBirth).TotalDays / 365.25);
        public Gender Gender { get; set; }
        public string? PhotoPath { get; set; }

        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string NationalId { get; set; } = string.Empty;

        public string EmergencyContactName { get; set; } = string.Empty;
        public string EmergencyContactPhone { get; set; } = string.Empty;
        public string? EmergencyContactRelation { get; set; }

        public string? MedicalNotes { get; set; }
        public decimal? HeightCm { get; set; }
        public decimal? WeightKg { get; set; }

        public decimal? Bmi => (HeightCm.HasValue && WeightKg.HasValue && HeightCm > 0)
            ? Math.Round(WeightKg.Value / (decimal)Math.Pow((double)(HeightCm.Value / 100), 2), 1)
            : null;

        public string? BmiCategory => Bmi switch
        {
            < 18.5m => "Underweight",
            < 25.0m => "Normal",
            < 30.0m => "Overweight",
            >= 30.0m => "Obese",
            _ => null
        };

        public DateTime JoinDate { get; set; } = DateTime.UtcNow;
        public bool IsAnonymised { get; set; } = false;

        // Navigation
        public ICollection<Membership> Memberships { get; set; } = [];
        public ICollection<Attendance> Attendances { get; set; } = [];
        public ICollection<ProgressRecord> ProgressRecords { get; set; } = [];
        public ICollection<TrainerAssignment> TrainerAssignments { get; set; } = [];
        public ICollection<TrainingProgram> TrainingPrograms { get; set; } = [];
        public ICollection<Notification> Notifications { get; set; } = [];
    }
}
