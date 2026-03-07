using System.ComponentModel.DataAnnotations;

namespace GymManagement.Web.ViewModels.Staf
{
    public class EditReceptionistViewModel
    {
        public Guid ReceptionistId { get; set; }

        [Required(ErrorMessage = "First name is required.")]
        [MaxLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [MaxLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone is required.")]
        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty;

        /// <summary>
        /// Permissions selected via checkboxes — each is one int bit value.
        /// Pre-populated by controller from existing Receptionist.Permissions bitmask.
        /// </summary>
        public List<int> SelectedPermissions { get; set; } = [];

        public int ComputedPermissions =>
            SelectedPermissions.Aggregate(0, (acc, p) => acc | p);
    }
}
