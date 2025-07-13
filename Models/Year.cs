using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;

namespace ExamTimeTable.Models
{
    public class Year
    {
        [Key]
        public int YearId { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        ////A Year of study belongs to a given department
        //public int? DepartmentId { get; set; }
        //public Department? Department { get; set; }

        //A year of study has a list of programmes under it
        public ICollection<Programme>? Programmes { get; set; }

        public ICollection<ProgrammeCourseUnit>? ProgrammeCourseUnits { get; set; }
        public ICollection<SubjectCombinationCourseUnit>? SubjectCombinationCourseUnits { get; set; }

        //Every year of study has a list of students under it
        public ICollection<ApplicationUser>? ApplicationUsers { get; set; }
    }
}