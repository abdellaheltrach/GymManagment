using GymManagement.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace GymManagement.Web.ViewModels.Trainers
{
    public class CreateTrainerViewModel
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Phone")]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [Display(Name = "Specialization")]
        public TrainerSpecialization Specialization { get; set; }

        [Display(Name = "Bio")]
        public string? Bio { get; set; }

        [Required]
        [Display(Name = "Years of Experience")]
        [Range(0, 50)]
        public int YearsOfExperience { get; set; }

        [Required]
        [Display(Name = "Salary Type")]
        public SalaryType SalaryType { get; set; }

        [Display(Name = "Base Salary")]
        [Range(0, 1000000)]
        public decimal? BaseSalary { get; set; }

        [Display(Name = "Commission Per Trainee")]
        [Range(0, 100000)]
        public decimal? CommissionPerTrainee { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Initial Password")]
        public string Password { get; set; } = string.Empty;
    }
}
