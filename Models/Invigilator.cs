using System.ComponentModel.DataAnnotations;

namespace ExamTimeTable.Models
{
    public class Invigilator
    {
        [Key]
        public int InvigilatorId { get; set; }

        [StringLength(50, ErrorMessage = "Invigilators is too long")]
        public string? Name { get; set; }

        [DataType(DataType.PhoneNumber)]
        public string? PhoneNumber { get; set; }

        //Relationship with ExamRoomInvigilator
        public ICollection<ExamRoomInvigilator> ExamRoomAssignments { get; set; } = new List<ExamRoomInvigilator>();

        //An invigilator belongs to a certain department
        public int DepartmentId { get; set; }
        public Department Department { get; set; }

        //Every year of study has a list of students under it
        public ICollection<ApplicationUser>? ApplicationUsers { get; set; }
    }
}
