namespace GymManagement.Web.ViewModels.Trainees
{
    public class EditTraineeViewModel : RegisterTraineeViewModel
    {
        public Guid TraineeId { get; set; }

        // NationalId is set at registration and cannot be changed.
        // Override to remove [Required] so validation doesn't block edits.
        public new string? NationalId { get; set; }

        // Emergency contact fields are not returned by TraineeDetailDto,
        // so override as optional for the edit form.
        public new string? EmergencyContactName { get; set; }
        public new string? EmergencyContactPhone { get; set; }
    }

}
