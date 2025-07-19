using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExamTimeTable.Models
{
    public class CourseUnit
    {
        [Key]
        public int CourseUnitId { get; set; }

        [Required]
        [StringLength(120, MinimumLength =5, ErrorMessage = "CourseUnit Name Should be greater than 5 characters")]
        public string Name { get; set; } = null!;

        [Required]
        public string Code { get; set; } = null!;

        [NotMapped]
        public int? EstimatedParticipants
        {
            get
            {
                int? fromProgrammes = ProgrammeCourseUnits
                    .Where(pcu => pcu.Programme != null)
                    .Sum(pcu => pcu.Programme?.NumberOfStudents);

                int? fromCombinations = SubjectCombinationCourseUnits
                    .Where(sccu => sccu.SubjectCombination != null)
                    .Sum(sccu => sccu.SubjectCombination?.NumberOfStudents);

                return fromProgrammes + fromCombinations;

            }
        }

        //Navigation Property for the many to many relationship between Programme 
        public ICollection<ProgrammeCourseUnit> ProgrammeCourseUnits { get; set; } = new List<ProgrammeCourseUnit>();

        //Navigation Property for the many to many relationship between SubjectCombination
        public ICollection<SubjectCombinationCourseUnit> SubjectCombinationCourseUnits { get; set; } = new List<SubjectCombinationCourseUnit>();

        //One to one relationship between Exam
        public int ExamId { get; set; }
        public Exam Exam { get; set; }
    }
}
