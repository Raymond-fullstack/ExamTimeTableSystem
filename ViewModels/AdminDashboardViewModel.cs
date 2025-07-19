using ExamTimeTable.Models;

namespace ExamTimeTable.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalDepartments { get; set; }
        public int TotalProgrammes { get; set; }
        public int TotalCourseUnits { get; set; }
        public int TotalRooms { get; set; }
        public int TotalInvigilators { get; set; }
        public List<Exam> UpcomingExams { get; set; } = new();
    }
}
