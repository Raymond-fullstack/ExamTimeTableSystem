using System.ComponentModel.DataAnnotations;

namespace ExamTimeTable.Models
{
    public class SubjectCombinationCourseUnit
    {
        public int SubjectCombinationCourseUnitId { get; set; }

        public int SubjectCombinationId { get; set; }
        public SubjectCombination SubjectCombination { get; set; }

        public int CourseUnitId { get; set; }
        public CourseUnit CourseUnit { get; set; }

        public int YearId { get; set; }
        public Year Year { get; set; }

        [MaxLength(10, ErrorMessage = "CourseUnit Code cannot exceed 10 characters")]
        public string CustomCode { get; set; } = string.Empty; // SubjectCombination-specific label
    }

}