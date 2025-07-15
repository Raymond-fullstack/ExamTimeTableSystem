namespace ExamTimeTable.ViewModels
{
    public class UserDisplayViewModel
    {
        public string Id { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = "User"; // Default fallback

        public string? Department { get; set; }

        public string? Programme { get; set; }

        public string? Year { get; set; }
    }
}
