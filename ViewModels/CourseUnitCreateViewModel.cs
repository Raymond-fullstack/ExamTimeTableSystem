using System.ComponentModel.DataAnnotations;

namespace ExamTimeTable.ViewModels
{
    public class CourseUnitCreateViewModel
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Code { get; set; } = string.Empty;

        // List of programmes excluding BSc. Education and HEAC
        public List<ProgrammeWithYearsViewModel> ProgrammesWithYears { get; set; } = new();

        // BSc. Education groupings
        public List<EducationYearWithCombinationsViewModel> EducationGroups { get; set; } = new();

        // HEAC special case
        public HEACWithCombinationsViewModel? HEACGroup { get; set; }

        public int? ExamId { get; set; }
    }

    public class ProgrammeWithYearsViewModel
    {
        public int ProgrammeId { get; set; }
        public string ProgrammeName { get; set; } = string.Empty;

        public List<YearCheckboxViewModel> Years { get; set; } = new();
    }

    public class EducationYearWithCombinationsViewModel
    {
        public int YearId { get; set; }
        public string YearName { get; set; } = string.Empty;

        public List<CombinationCheckboxViewModel> Combinations { get; set; } = new();
    }

    public class HEACWithCombinationsViewModel
    {
        public int ProgrammeId { get; set; }
        public string ProgrammeName { get; set; } = "HEAC";

        public List<CombinationCheckboxViewModel> Combinations { get; set; } = new();
    }

    public class YearCheckboxViewModel
    {
        public int YearId { get; set; }
        public string YearName { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }

    public class CombinationCheckboxViewModel
    {
        public int SubjectCombinationId { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }
}
