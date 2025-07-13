using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExamTimeTable.Models
{
    public class Exam
    {
        [Key]
        public int ExamId { get; set; }
        public DateOnly ExamDate { get; set; }

        [Range(1,3)]
        public int Duration { get; set; }
        public TimeOnly StartTime { get; set; }

        [NotMapped]
        public TimeOnly EndTime => StartTime.AddHours(Duration);

        [StringLength(100, MinimumLength =20, ErrorMessage = "Be more detailed while giving instructions")]
        public string? SpecialInstructions { get; set; }

        //One to one relationship with CourseUnit
        public int CourseUnitId { get; set; }
        public CourseUnit CourseUnit { get; set; }

        //Navigation Property for many to many relationship with rooms
        public ICollection<ExamRoom> ExamRooms { get; set; }
    }
}