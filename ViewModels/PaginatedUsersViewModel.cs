using ExamTimeTable.ViewModels;

public class PaginatedUsersViewModel
{
    public List<UserListViewModel> Users { get; set; } = new();
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public string? SearchQuery { get; set; }
}
