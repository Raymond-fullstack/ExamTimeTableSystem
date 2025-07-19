using System.ComponentModel.DataAnnotations;

namespace ExamTimeTable.Models
{
    public class Room
    {
        [Key]
        public int RoomId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Block { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public ICollection<ExamRoom> ExamRooms { get; set; } = new List<ExamRoom>();

        //One to many with invigilators
        public ICollection<Invigilator>? Invigilators { get; set; }
    }
}
