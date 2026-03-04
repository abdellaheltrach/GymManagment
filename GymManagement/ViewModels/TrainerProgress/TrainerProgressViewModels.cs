using GymManagement.Application.Common.DTOs;
using System.ComponentModel.DataAnnotations;

namespace GymManagement.Web.ViewModels.TrainerProgress;

public class RecordProgressViewModel
{
    public Guid TraineeId { get; set; }
    public string TraineeName { get; set; } = string.Empty;

    [Display(Name = "Weight (kg)")]
    [Range(20, 500)] public decimal? WeightKg { get; set; }

    [Display(Name = "Body Fat (%)")]
    [Range(2, 70)] public decimal? BodyFatPercent { get; set; }

    [Display(Name = "Muscle Mass (kg)")]
    [Range(10, 200)] public decimal? MuscleMassKg { get; set; }

    [Display(Name = "Chest (cm)")]
    [Range(40, 200)] public decimal? ChestCm { get; set; }

    [Display(Name = "Waist (cm)")]
    [Range(40, 200)] public decimal? WaistCm { get; set; }

    [Display(Name = "Hips (cm)")]
    [Range(40, 200)] public decimal? HipsCm { get; set; }

    [Display(Name = "Arm (cm)")]
    [Range(10, 100)] public decimal? ArmCm { get; set; }

    [Display(Name = "Notes")]
    [MaxLength(500)] public string? Notes { get; set; }
}

public class ProgressHistoryViewModel
{
    public Guid TraineeId { get; init; }
    public string TraineeName { get; init; } = string.Empty;
    public IReadOnlyList<ProgressRecordDto> Records { get; init; } = [];

    public decimal? WeightDelta { get; init; }
    public decimal? BodyFatDelta { get; init; }
    public decimal? MuscleDelta { get; init; }
    public decimal? WaistDelta { get; init; }

    public bool HasRecords => Records.Count > 0;

    // Returns CSS class + arrow symbol for a delta value.
    // lowerIsBetter = true for weight/fat/waist
    public static (string css, string arrow) DeltaBadge(
        decimal? delta, bool lowerIsBetter = false)
    {
        if (delta is null) return ("badge bg-secondary", "—");
        bool positive = delta > 0;
        bool good = lowerIsBetter ? !positive : positive;
        string css = good ? "badge bg-success" : "badge bg-danger";
        string arrow = positive ? $"+{delta:F1}" : $"{delta:F1}";
        return (css, arrow);
    }
}

public class TrainerProgressIndexViewModel
{
    public List<TraineeProgressSummaryViewModel> Trainees { get; set; } = new();
}

public class TraineeProgressSummaryViewModel
{
    public Guid TraineeId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public decimal? LatestWeight { get; set; }
    public DateTime? LatestRecordedAt { get; set; }
}
