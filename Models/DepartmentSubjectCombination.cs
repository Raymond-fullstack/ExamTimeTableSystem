namespace ExamTimeTable.Models
{
    public class DepartmentSubjectCombination
    {
        public int DepartmentId { get; set; }
        public Department? Department { get; set; }

        public int SubjectCombinationId { get; set; }
        public SubjectCombination? SubjectCombination { get; set; }
    }
}