﻿namespace ExamTimeTable.Models
{
    public class ExamRoom
    {
        public int ExamId { get; set; }
        public Exam Exam { get; set; } 

        public int RoomId { get; set; }
        public Room Room { get; set; }
    }
}