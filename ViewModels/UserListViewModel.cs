using ExamTimeTable.ViewModels;

public class UserListViewModel
{
    public List<UserDisplayViewModel> AdminsAndInvigilators { get; set; } = new();
    public List<UserDisplayViewModel> Students { get; set; } = new();

    public int AdminCurrentPage { get; set; }
    public int AdminTotalPages { get; set; }

    public int StudentCurrentPage { get; set; }
    public int StudentTotalPages { get; set; }

    public string SearchQuery { get; set; } = "";
}
