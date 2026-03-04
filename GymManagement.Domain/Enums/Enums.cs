namespace GymManagement.Domain.Enums
{

    public enum MembershipStatus
    {
        PendingPayment,
        Active,
        Frozen,
        Expired,
        Suspended,
        /// <summary>
        /// Membership was cancelled before its natural expiry.
        /// May have a full or partial refund recorded against it.
        /// Trainer assignment is auto-removed on cancellation.
        /// </summary>
        Cancelled
    }

    public enum PaymentMethod
    {
        Cash,
        Card,
        BankTransfer
    }

    public enum PaymentStatus
    {
        Pending,
        Paid,
        PartiallyPaid,
        Refunded
    }

    public enum TrainerSpecialization
    {
        Cardio,
        Strength,
        Bodybuilding,
        Rehabilitation,
        Yoga,
        CrossFit,
        Nutrition
    }

    public enum SalaryType
    {
        Fixed,
        Commission,
        Hybrid
    }

    public enum AccessLevel
    {
        Basic,
        Standard,
        Premium,
        VIP
    }

    public enum AttendanceMethod
    {
        Manual,
        QRCode,
        CardScan
    }

    public enum NotificationType
    {
        MembershipExpirySoon,
        MembershipExpired,
        PaymentReceived,
        PaymentDue,
        TrainerAssigned,
        AccountSuspended,
        AccountActivated,
        General
    }

    public enum AuditAction
    {
        Created,
        Updated,
        Deleted,
        Restored
    }

    public enum Gender
    {
        Male,
        Female,
    }
}
