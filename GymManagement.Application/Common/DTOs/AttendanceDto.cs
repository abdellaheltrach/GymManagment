using GymManagement.Domain.Enums;

namespace GymManagement.Application.Common.DTOs;

// ── Attendance DTOs ───────────────────────────────────────────────────────────

public record AttendanceDto(
    Guid Id,
    Guid TraineeId,
    string TraineeName,
    DateTime CheckInTime,
    DateTime? CheckOutTime,
    TimeSpan? Duration,
    AttendanceMethod Method
);
