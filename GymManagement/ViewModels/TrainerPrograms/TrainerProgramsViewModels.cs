using GymManagement.Application.Common.DTOs;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace GymManagement.Web.ViewModels.TrainerPrograms;

public class TrainerProgramsListViewModel
{
    public IReadOnlyList<TrainingProgramDto> Programs { get; init; } = [];
    public int ActiveCount => Programs.Count(p => p.IsActive);
    public int InactiveCount => Programs.Count(p => !p.IsActive);
}

public class CreateProgramViewModel
{
    [Required] public Guid TraineeId { get; set; }
    [Required][MaxLength(200)] public string Title { get; set; } = string.Empty;
    [MaxLength(1000)] public string? Description { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Start Date")] public DateTime StartDate { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "End Date")] public DateTime? EndDate { get; set; }

    public List<SelectListItem> TraineeList { get; set; } = [];
}

public class ProgramDetailViewModel
{
    public required TrainingProgramDto Program { get; init; }
    public required AddExerciseViewModel AddExerciseForm { get; init; }
}

public class AddExerciseViewModel
{
    public Guid ProgramId { get; set; }

    [Required][MaxLength(100)] public string Name { get; set; } = string.Empty;

    [Required]
    [Range(1, 100)]
    [Display(Name = "Sets")] public int Sets { get; set; } = 3;

    [Required]
    [Range(1, 1000)]
    [Display(Name = "Reps")] public int Reps { get; set; } = 10;

    [Range(0, 500)]
    [Display(Name = "Weight (kg)")] public decimal? WeightKg { get; set; }

    [Display(Name = "Duration (sec)")] public int? DurationSeconds { get; set; }
    [Display(Name = "Rest (sec)")] public int? RestSeconds { get; set; }
    [MaxLength(300)] public string? Notes { get; set; }
}
