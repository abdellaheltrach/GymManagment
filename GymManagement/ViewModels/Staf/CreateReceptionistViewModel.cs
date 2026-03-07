using GymManagement.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace GymManagement.Web.ViewModels.Staf
{
    public class CreateReceptionistViewModel
    {
        [Required(ErrorMessage = "First name is required.")]
        [MaxLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [MaxLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone is required.")]
        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(12, ErrorMessage = "Password must be at least 12 characters.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm the password.")]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        /// <summary>
        /// Permissions selected via checkboxes.
        /// Each checkbox posts the int value of one ReceptionistPermission bit.
        /// </summary>
        public List<int> SelectedPermissions { get; set; } = [];

        /// <summary>
        /// Computed bitmask from selected checkboxes.
        /// Falls back to StandardDesk if nothing selected.
        /// </summary>
        public int ComputedPermissions =>
            SelectedPermissions.Count > 0
                ? SelectedPermissions.Aggregate(0, (acc, p) => acc | p)
                : (int)ReceptionistPermission.StandardDesk;
    }
}