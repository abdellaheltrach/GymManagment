using GymManagement.Domain.Enums;

namespace GymManagement.Web.ViewModels.Staf
{
    public class ReceptionistDetailViewModel
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public DateTime HireDate { get; set; }
        public bool IsActive { get; set; }
        public ReceptionistPermission Permissions { get; set; }
        public DateTime CreatedAt { get; set; }

        public bool HasPermission(ReceptionistPermission p)
            => (Permissions & p) == p;

        public string StatusBadge => IsActive ? "badge bg-success" : "badge bg-secondary";
        public string StatusLabel => IsActive ? "Active" : "Inactive";
    }
}
