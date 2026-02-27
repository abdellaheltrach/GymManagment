using GymManagement.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace GymManagement.Web.ViewModels.Trainees
{
    public class RegisterTraineeViewModel
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
        [Display(Name = "National ID")]
        public string NationalId { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [Display(Name = "Gender")]
        public Gender Gender { get; set; }

        [Display(Name = "Address")]
        public string? Address { get; set; }

        [Display(Name = "Medical Notes")]
        public string? MedicalNotes { get; set; }

        [Display(Name = "Height (cm)")]
        [Range(50, 300)]
        public decimal? HeightCm { get; set; }

        [Display(Name = "Weight (kg)")]
        [Range(20, 500)]
        public decimal? WeightKg { get; set; }

        [Required]
        [Display(Name = "Emergency Contact Name")]
        public string EmergencyContactName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Emergency Contact Phone")]
        public string EmergencyContactPhone { get; set; } = string.Empty;

        [Display(Name = "Relation")]
        public string? EmergencyContactRelation { get; set; }
    }

}
