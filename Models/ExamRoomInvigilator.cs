namespace ExamTimeTable.Models
{
    public class ExamRoomInvigilator
    {
        public int ExamId { get; set; }
        public int RoomId { get; set; }

        public ExamRoom ExamRoom { get; set; } = null!;

        public int InvigilatorId { get; set; }
        public Invigilator Invigilator { get; set; } = null!;
    }

}