using ExamTimeTable.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ExamTimeTable.ViewModels
{
    public class CourseUnitIndexViewModel
    {
        public List<CourseUnit> CourseUnits { get; set; } = new List<CourseUnit>();

        // Dropdown data
        public SelectList Programmes { get; set; }
        public SelectList SubjectCombinations { get; set; }
        public SelectList Years { get; set; }

        // Currently selected filters
        public int? SelectedProgrammeId { get; set; }
        public int? SelectedSubjectCombinationId { get; set; }
        public int? SelectedYearId { get; set; }
        public string? SelectedSharedFilter { get; set; }
    }
}
