using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExamTimeTable.Models
{
    public class SubjectCombination
    {
        [Key]
        public int SubjectCombinationId { get; set; }

        [Required]
        [MaxLength(20, ErrorMessage = "SubjectCombination Name should be less than 10 characters")]
        public string Name { get; set; } = null!;

        [DisplayName("Number Of Students")]
        public int NumberOfStudents { get; set; }

        //A subjectcombination belongs to a given programme
        public int ProgrammeId { get; set; }
        public Programme? Programme { get; set; }

        //Navigation property for many to many relationship between Department
        public ICollection<DepartmentSubjectCombination> DepartmentSubjectCombinations { get; set; } = new List<DepartmentSubjectCombination>();

        //Navigation property for many to many relationship between CourseUnits
        public ICollection<SubjectCombinationCourseUnit> SubjectCombinationCourseUnits { get; set; } = new List<SubjectCombinationCourseUnit>();

        [NotMapped]
        public List<int> SelectedDepartmentIds { get; set; } = new List<int>();
    }
}