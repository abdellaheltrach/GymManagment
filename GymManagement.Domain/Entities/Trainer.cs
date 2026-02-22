using GymManagement.Domain.Bases;
using GymManagement.Domain.Enums;

namespace GymManagement.Domain.Entities
{

    public class Trainer : AuditableEntity
    {
        public string ApplicationUserId { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";

        public DateTime DateOfBirth { get; set; }
        public Gender Gender { get; set; }
        public string? PhotoPath { get; set; }

        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;

        public TrainerSpecialization Specialization { get; set; }
        public string? Bio { get; set; }
        public int YearsOfExperience { get; set; }
        public DateTime HireDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        public SalaryType SalaryType { get; set; }
        public decimal? BaseSalary { get; set; }
        public decimal? CommissionPerTrainee { get; set; }

        // Navigation
        public ICollection<TrainerAssignment> TrainerAssignments { get; set; } = [];
        public ICollection<TrainingProgram> TrainingPrograms { get; set; } = [];
    }
}
