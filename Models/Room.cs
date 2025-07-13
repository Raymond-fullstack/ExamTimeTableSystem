using System.ComponentModel.DataAnnotations;

namespace ExamTimeTable.Models
{
    public class Room
    {
        [Key]
        public int RoomId { get; set; }
        public string Code { get; set; }
        public int Capacity { get; set; }
        public ICollection<ExamRoom> ExamRooms { get; set; }
    }
}