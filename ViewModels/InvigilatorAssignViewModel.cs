using ExamTimeTable.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ExamTimeTable.ViewModels
{
    public class InvigilatorAssignViewModel
    {
        public string? SelectedUserId { get; set; }

        public List<SelectListItem> AvailableInvigilators { get; set; } = new();
    }

}
