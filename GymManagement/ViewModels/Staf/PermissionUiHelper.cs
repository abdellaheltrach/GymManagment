using GymManagement.Domain.Enums;

namespace GymManagement.Web.ViewModels.Staf
{
    public static class PermissionUiHelper
    {
        public static IReadOnlyList<(ReceptionistPermission Value, string Label, string Description)>
            AllPermissions =>
        [
            (ReceptionistPermission.CheckInTrainees,      "Check In / Out Members",
                "Mark attendance for trainees at the front desk."),
            (ReceptionistPermission.RegisterTrainees,     "Register New Members",
                "Create new trainee accounts and profiles."),
            (ReceptionistPermission.AssignMemberships,    "Assign Memberships",
                "Assign membership plans to trainees."),
            (ReceptionistPermission.RecordPayments,       "Record Payments",
                "Record cash or card payments against memberships."),
            (ReceptionistPermission.FreezeMemberships,    "Freeze Memberships",
                "Pause an active membership on member request."),
            (ReceptionistPermission.EditTraineeProfile,   "Edit Trainee Profile",
                "Update trainee personal info, height, weight, notes."),
            (ReceptionistPermission.SuspendMemberships,   "Suspend Memberships",
                "Administratively suspend a membership."),
            (ReceptionistPermission.CancelMemberships,    "Cancel Memberships",
                "Cancel a membership with full or partial refund."),
            (ReceptionistPermission.ViewFinancialReports, "View Financial Reports",
                "Access revenue reports and payment summaries.")
        ];
    }
}
