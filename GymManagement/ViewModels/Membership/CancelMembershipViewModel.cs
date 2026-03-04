using GymManagement.Application.Features.Memberships.Commands.CancelMembership;
using GymManagement.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace GymManagement.Web.ViewModels.Memberships;

public class CancelMembershipViewModel
{
    public Guid   MembershipId  { get; set; }
    public Guid   TraineeId     { get; set; }
    public string TraineeName   { get; set; } = string.Empty;
    public string PlanName      { get; set; } = string.Empty;
    public decimal AmountPaid   { get; set; }
    public decimal SuggestedRefund { get; set; } // unused days proportion

    [Required(ErrorMessage = "Please select a refund type.")]
    public RefundType RefundType { get; set; }

    [Range(0.01, 1_000_000, ErrorMessage = "Enter a valid refund amount.")]
    public decimal? RefundAmount { get; set; }

    [Required]
    public PaymentMethod RefundMethod { get; set; }

    [Required(ErrorMessage = "Cancellation reason is required.")]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
}
