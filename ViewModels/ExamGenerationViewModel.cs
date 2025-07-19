using System;
using System.ComponentModel.DataAnnotations;

namespace ExamTimeTable.ViewModels
{
    public class ExamGenerationViewModel
    {
        [Required(ErrorMessage = "Please select a valid exam start date.")]
        [DataType(DataType.Date)]
        public DateTime ExamStartDate { get; set; }
    }
}
