using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ExamTimeTable.Models
{
    public class Programme
    {
        [Key]
        public int ProgrammeId { get; set; }

        [Required]
        [MaxLength(50, ErrorMessage = "Programme Name should be less than 50 characters")]
        public string Name { get; set; } = null!;

        [DisplayName("Number Of Students")]
        public int? NumberOfStudents { get; set; }

        //A programme belongs to a certain year of study
        public int YearId { get; set; }
        public Year? Year { get; set; }

        //A programme belongs to a certain department
        public int DepartmentId { get; set; }
        public Department? Department { get; set; }

        //A programme may have a list of subjectcombinations under it
        public ICollection<SubjectCombination>? SubjectCombinations { get; set; }

        //Navigation property for many to many relationship between CourseUnits
        public ICollection<ProgrammeCourseUnit>? ProgrammeCourseUnits { get; set; }

        //Every year of study has a list of students under it
        public ICollection<ApplicationUser>? ApplicationUsers { get; set; }
    }
}