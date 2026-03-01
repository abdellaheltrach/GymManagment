using GymManagement.Application.Common.DTOs;
using GymManagement.Domain.Entities;

namespace GymManagement.Application.Common.Mappings;

public partial class MappingProfile
{
    private void ConfigurePaymentMappings()
    {
        CreateMap<Payment, PaymentDto>()
            .ConstructUsing((src, ctx) => new PaymentDto(
                src.Id,
                src.MembershipId,
                src.Membership?.Trainee?.FullName ?? "Unknown Trainee",
                src.Membership?.Plan?.Name ?? "Unknown Plan",
                src.Amount,
                src.Method,
                src.Status,
                src.PaidAt,
                src.ReferenceNumber,
                src.IsRefunded,
                src.RefundedAt,
                src.RefundReason
            ));
    }
}
