using GymManagement.Domain.Enums;

namespace GymManagement.Application.Common.DTOs
{
    public record ReceptionistDetailDto(
        Guid Id,
        string FirstName,
        string LastName,
        string Email,
        string Phone,
        DateTime HireDate,
        bool IsActive,
        ReceptionistPermission Permissions,
        DateTime CreatedAt
    );
}
