namespace GymManagement.Web.ViewModels.Shared
{
    public class PaginationViewModel
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public string? SearchTerm { get; set; }
        public string ActionName { get; set; } = "Index";
        public string ControllerName { get; set; } = string.Empty;

        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }
}
