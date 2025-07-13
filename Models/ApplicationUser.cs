using Microsoft.AspNetCore.Identity;

namespace ExamTimeTable.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? StudentNumber { get; set; }

        //A user can be a student or an invigilator
        //An Invigilator has to belong to a certain department
        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }

        //A student user belongs to a certain programme
        public int? ProgrammeId { get; set; }
        public Programme? Programme { get; set; }

        //A student user belongs to a certain year of study
        public int? YearId { get; set; }
        public Year? Year { get; set; }

        public int? InvigilatorId { get; set; }
        public Invigilator? Invigilator { get; set; }
        public string? ProfilePicture { get; set; } = "default.png"; // Default profile picture
    }
}