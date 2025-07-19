using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ExamTimeTable.Models
{
    public class Department
    {
        [Key]
        public int DepartmentId { get; set; }

        [Required]
        [MaxLength(50, ErrorMessage = "Department Name Can't Exceed 20 characters")]
        public string Name { get; set; } = null!;

        [MaxLength(200, ErrorMessage ="You've reached your description limit!.")]
        public string? Description { get; set; }

        ////A Department handles multiple years of study
        //public ICollection<Year>? Years { get; set; }

        //Many Programmmes can be under a given department
        public ICollection<Programme>? Programmes { get; set; }

        //An invigilator may belong to a given number
        public ICollection<Invigilator>? Invigilators { get; set; }

        //A user can belong to a given department if applicable
        public ICollection<ApplicationUser>? ApplicationUsers { get; set; }

        //Navigation Property for many to many relationship between subject combination
        public ICollection<DepartmentSubjectCombination>? DepartmentSubjectCombinations { get; set; }

    }
}
