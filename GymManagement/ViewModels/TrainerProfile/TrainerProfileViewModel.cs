using GymManagement.Application.Features.Trainers.Queries.GetMyProfile;

namespace GymManagement.Web.ViewModels.TrainerProfile;

public class TrainerProfileViewModel
{
    public required TrainerProfileDto Profile { get; init; }

    public string SalaryDisplay => Profile.SalaryType switch
    {
        "Fixed"      => Profile.BaseSalary.HasValue
                         ? $"{Profile.BaseSalary:C} / month"
                         : "Fixed (not set)",
        "Commission" => Profile.CommissionPerTrainee.HasValue
                         ? $"{Profile.CommissionPerTrainee:C} per trainee"
                         : "Commission (not set)",
        _             => Profile.SalaryType
    };

    public string YearsLabel =>
        Profile.YearsOfExperience == 1 ? "1 year" :
        $"{Profile.YearsOfExperience} years";
}
