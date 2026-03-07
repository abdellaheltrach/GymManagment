using GymManagement.Domain.Bases;
using GymManagement.Domain.Enums;

namespace GymManagement.Domain.Entities
{
    /// <summary>
    /// Represents a front-desk staff member.
    /// Linked 1-to-1 with ApplicationUser (holds Identity/auth).
    /// Permissions is a bitmask (ReceptionistPermission flags enum) stored as int.
    /// </summary>
    public class Receptionist : AuditableEntity
    {
        public string ApplicationUserId { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";

        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;

        public DateTime HireDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Bitmask of ReceptionistPermission flags.
        /// Default = StandardDesk (register, assign, pay, freeze, check-in).
        /// </summary>
        public int Permissions { get; set; } =
            (int)ReceptionistPermission.StandardDesk;

        // ── Helpers (not persisted) ────────────────────────────────────────
        public bool HasPermission(ReceptionistPermission permission)
            => (Permissions & (int)permission) == (int)permission;

        public void GrantPermission(ReceptionistPermission permission)
            => Permissions |= (int)permission;

        public void RevokePermission(ReceptionistPermission permission)
            => Permissions &= ~(int)permission;

        public void SetPermissions(ReceptionistPermission permissions)
            => Permissions = (int)permissions;
    }
}
