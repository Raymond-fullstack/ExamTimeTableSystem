using System.ComponentModel.DataAnnotations;

namespace ExamTimeTable.Models
{
    public class ExamRoom
    {
        //[Key]
        //public int ExamRoomId { get; set; } no necessary as it is not used as a primary key

        public int ExamId { get; set; }
        public Exam Exam { get; set; } 

        public int RoomId { get; set; }
        public Room Room { get; set; }

        //Navigation property for many-to-many relationship with ExamRoomInvigilator
        public ICollection<ExamRoomInvigilator> Invigilators { get; set; } = new List<ExamRoomInvigilator>();
    }
}