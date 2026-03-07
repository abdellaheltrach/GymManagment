using GymManagement.Domain.Enums;

namespace GymManagement.Application.Common.DTOs
{
    public record ReceptionistSummaryDto(
        Guid Id,
        string FullName,
        string Email,
        string Phone,
        DateTime HireDate,
        bool IsActive,
        ReceptionistPermission Permissions
    );
}
