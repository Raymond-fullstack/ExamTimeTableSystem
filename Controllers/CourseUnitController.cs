using ExamTimeTable.Data;
using ExamTimeTable.Models;
using ExamTimeTable.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ExamTimeTable.Controllers
{
    [Authorize(Roles = "SystemAdmin, Admin")]
    public class CourseUnitController : Controller
    {
        private readonly ApplicationDbContext _context;
        public CourseUnitController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
            int? programmeId,
            int? subjectCombinationId,
            int? yearId,
            string? sharedFilter)
                {
            // Step 1: Load all course units with needed includes
            var courseUnitsQuery = _context.CourseUnits
                .Include(cu => cu.Exam)
                .Include(cu => cu.ProgrammeCourseUnits)
                    .ThenInclude(pcu => pcu.Programme)
                .Include(cu => cu.ProgrammeCourseUnits)
                    .ThenInclude(pcu => pcu.Year)
                .Include(cu => cu.SubjectCombinationCourseUnits)
                    .ThenInclude(sccu => sccu.SubjectCombination)
                .Include(cu => cu.SubjectCombinationCourseUnits)
                    .ThenInclude(sccu => sccu.Year)
                .AsQueryable();

            // Step 2: Apply filters
            if (programmeId.HasValue)
            {
                courseUnitsQuery = courseUnitsQuery.Where(cu =>
                    cu.ProgrammeCourseUnits.Any(pcu => pcu.ProgrammeId == programmeId));
            }

            if (subjectCombinationId.HasValue)
            {
                courseUnitsQuery = courseUnitsQuery.Where(cu =>
                    cu.SubjectCombinationCourseUnits.Any(sccu => sccu.SubjectCombinationId == subjectCombinationId));
            }

            if (yearId.HasValue)
            {
                courseUnitsQuery = courseUnitsQuery.Where(cu =>
                    cu.ProgrammeCourseUnits.Any(pcu => pcu.YearId == yearId) ||
                    cu.SubjectCombinationCourseUnits.Any(sccu => sccu.YearId == yearId));
            }

            if (!string.IsNullOrEmpty(sharedFilter))
            {
                if (sharedFilter == "shared")
                {
                    courseUnitsQuery = courseUnitsQuery.Where(cu =>
                        cu.ProgrammeCourseUnits.Count + cu.SubjectCombinationCourseUnits.Count > 1);
                }
                else if (sharedFilter == "exclusive")
                {
                    courseUnitsQuery = courseUnitsQuery.Where(cu =>
                        cu.ProgrammeCourseUnits.Count + cu.SubjectCombinationCourseUnits.Count <= 1);
                }
            }

            // Step 3: Get filter dropdown values
            var programmes = await _context.Programmes
                .Include(p => p.Year)
                .OrderBy(p => p.Name)
                .ToListAsync();

            var combinations = await _context.SubjectCombinations
                .OrderBy(c => c.Name)
                .ToListAsync();

            var years = await _context.Years.OrderBy(y => y.Name).ToListAsync();

            // Step 4: Create ViewModel (next step we'll define it)
            var viewModel = new CourseUnitIndexViewModel
            {
                CourseUnits = await courseUnitsQuery.ToListAsync(),
                Programmes = new SelectList(programmes.Select(p => new
                {
                    p.ProgrammeId,
                    Display = p.Name + " - " + (p.Year != null ? p.Year.Name : "No Year")
                }), "ProgrammeId", "Display"),

                SubjectCombinations = new SelectList(combinations, "SubjectCombinationId", "Name"),
                Years = new SelectList(years, "YearId", "Name"),

                SelectedProgrammeId = programmeId,
                SelectedSubjectCombinationId = subjectCombinationId,
                SelectedYearId = yearId,
                SelectedSharedFilter = sharedFilter
            };

            return View(viewModel);
        }

        public IActionResult Create()
        {
            var viewModel = new CourseUnitCreateViewModel();

            // 1. Load all years
            var years = _context.Years.ToList();

            // 2. Load all programmes with their years
            var allProgrammes = _context.Programmes.Include(p => p.Year).ToList();

            // 3. Identify special programmes
            var educationProgrammes = allProgrammes
                .Where(p => p.Name.Contains("Bsc. Education", StringComparison.OrdinalIgnoreCase))
                .Select(p => p.ProgrammeId)
                .ToHashSet();

            var heacProgramme = allProgrammes
                .FirstOrDefault(p => p.Name.Equals("Higher Education Access Certificate", StringComparison.OrdinalIgnoreCase));

            // 4. Normal programmes (excluding Education and HEAC)
            viewModel.ProgrammesWithYears = allProgrammes
                .Where(p => !educationProgrammes.Contains(p.ProgrammeId) && p != heacProgramme)
                .GroupBy(p => p.Name)
                .Select(g => new ProgrammeWithYearsViewModel
                {
                    ProgrammeName = g.Key,
                    ProgrammeId = g.First().ProgrammeId, // use first id
                    Years = g
                        .Where(p => p.Year != null && p.Year.Name != "HEAC")
                        .Select(p => new YearCheckboxViewModel
                        {
                            YearId = p.Year!.YearId,
                            YearName = p.Year.Name
                        }).ToList()
                }).ToList();

            // 5. BSc. Education — Group by year and load subject combinations
            var educationCombinations = _context.SubjectCombinations
                .Include(sc => sc.Programme)
                .Where(sc => educationProgrammes.Contains(sc.ProgrammeId) && sc.Programme!.Name.Contains("Bsc. Education"))
                .ToList();

            viewModel.EducationGroups = educationCombinations
                .GroupBy(sc => sc.Programme!.YearId)
                .Select(g => new EducationYearWithCombinationsViewModel
                {
                    YearId = g.Key,
                    YearName = years.FirstOrDefault(y => y.YearId == g.Key)?.Name ?? "Unknown Year",
                    Combinations = g.Select(sc => new CombinationCheckboxViewModel
                    {
                        SubjectCombinationId = sc.SubjectCombinationId,
                        Name = sc.Name
                    }).ToList()
                }).ToList();

            // 6. HEAC Programme and its combinations
            if (heacProgramme != null)
            {
                var heacCombinations = _context.SubjectCombinations
                    .Where(sc => sc.ProgrammeId == heacProgramme.ProgrammeId)
                    .Select(sc => new CombinationCheckboxViewModel
                    {
                        SubjectCombinationId = sc.SubjectCombinationId,
                        Name = sc.Name
                    }).ToList();

                viewModel.HEACGroup = new HEACWithCombinationsViewModel
                {
                    ProgrammeId = heacProgramme.ProgrammeId,
                    Combinations = heacCombinations
                };
            }

            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CourseUnitCreateViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                // You can re-populate the viewModel here if needed (like in GET)
                return View(viewModel);
            }

            var courseUnit = new CourseUnit
            {
                Name = viewModel.Name,
                Code = viewModel.Code,
                ProgrammeCourseUnits = new List<ProgrammeCourseUnit>(),
                SubjectCombinationCourseUnits = new List<SubjectCombinationCourseUnit>()
            };

            // Add Programme ↔ CourseUnit relations (with YearId)
            foreach (var prog in viewModel.ProgrammesWithYears)
            {
                foreach (var year in prog.Years.Where(y => y.IsSelected))
                {
                    courseUnit.ProgrammeCourseUnits.Add(new ProgrammeCourseUnit
                    {
                        ProgrammeId = prog.ProgrammeId,
                        YearId = year.YearId
                    });
                }
            }

            // Add Education Subject Combinations ↔ CourseUnit relations
            foreach (var edu in viewModel.EducationGroups)
            {
                foreach (var combo in edu.Combinations.Where(c => c.IsSelected))
                {
                    courseUnit.SubjectCombinationCourseUnits.Add(new SubjectCombinationCourseUnit
                    {
                        SubjectCombinationId = combo.SubjectCombinationId,
                        YearId = edu.YearId
                    });
                }
            }

            // Add HEAC Subject Combinations ↔ CourseUnit relations
            if (viewModel.HEACGroup != null)
            {
                var heacYearId = await _context.Years
                    .Where(y => y.Name.ToLower() == "heac")
                    .Select(y => y.YearId)
                    .FirstOrDefaultAsync();

                foreach (var combo in viewModel.HEACGroup.Combinations.Where(c => c.IsSelected))
                {
                    courseUnit.SubjectCombinationCourseUnits.Add(new SubjectCombinationCourseUnit
                    {
                        SubjectCombinationId = combo.SubjectCombinationId,
                        YearId = heacYearId
                    });
                }
            }

            _context.CourseUnits.Add(courseUnit);
            await _context.SaveChangesAsync();

            TempData["ToastMessage"] = "Course Unit created successfully!";
            TempData["ToastType"] = "success";

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var courseUnit = await _context.CourseUnits
                .Include(cu => cu.ProgrammeCourseUnits)
                .Include(cu => cu.SubjectCombinationCourseUnits)
                .FirstOrDefaultAsync(cu => cu.CourseUnitId == id);

            if (courseUnit == null)
            {
                return NotFound();
            }

            var viewModel = new CourseUnitEditViewModel
            {
                CourseUnitId = courseUnit.CourseUnitId,
                Name = courseUnit.Name,
                Code = courseUnit.Code
            };

            // Programmes excluding Education and HEAC
            var nonEduProgrammes = _context.Programmes
                .Include(p => p.Year)
                .Where(p => p.Name != "Bsc. Education" && p.Name != "Higher Education Access Certificate")
                .ToList();

            viewModel.ProgrammesWithYears = nonEduProgrammes
                .GroupBy(p => p.Name)
                .Select(g => new ProgrammeWithYearsViewModel
                {
                    ProgrammeId = g.First().ProgrammeId,
                    ProgrammeName = g.Key,
                    Years = g.Select(p => new YearCheckboxViewModel
                    {
                        YearId = p.YearId,
                        YearName = p.Year!.Name,
                        IsSelected = courseUnit.ProgrammeCourseUnits.Any(pcu =>
                            pcu.ProgrammeId == p.ProgrammeId && pcu.YearId == p.YearId)
                    }).ToList()
                }).ToList();

            // Education Grouped by Year
            var educationYears = _context.Years
                .Where(y => y.Name != "HEAC")
                .ToList();

            viewModel.EducationGroups = educationYears
            .AsEnumerable()
            .Select(y => new EducationYearWithCombinationsViewModel
            {
                YearId = y.YearId,
                YearName = y.Name,
                Combinations = _context.SubjectCombinations
                    .Where(sc => sc.Programme!.Name == "Bsc. Education" && sc.Programme.YearId == y.YearId)
                    .ToList()
                    .Select(sc => new CombinationCheckboxViewModel
                    {
                        SubjectCombinationId = sc.SubjectCombinationId,
                        Name = sc.Name,
                        IsSelected = courseUnit.SubjectCombinationCourseUnits
                            .Any(sccu => sccu.SubjectCombinationId == sc.SubjectCombinationId && sccu.YearId == y.YearId)
                    })
                    .ToList()
            })
            .ToList();


            // HEAC
            var heacProgramme = await _context.Programmes
                .Include(p => p.Year)
                .FirstOrDefaultAsync(p => p.Name == "Higher Education Access Certificate");

            if (heacProgramme != null)
            {
                viewModel.HEACGroup = new HEACWithCombinationsViewModel
                {
                    ProgrammeId = heacProgramme.ProgrammeId,
                    Combinations = _context.SubjectCombinations
                    .Where(sc => sc.ProgrammeId == heacProgramme.ProgrammeId)
                    .AsEnumerable()
                    .Select(sc => new CombinationCheckboxViewModel
                    {
                        SubjectCombinationId = sc.SubjectCombinationId,
                        Name = sc.Name,
                        IsSelected = courseUnit.SubjectCombinationCourseUnits
                            .Any(sccu => sccu.SubjectCombinationId == sc.SubjectCombinationId)
                    })
                    .ToList()
                };

            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CourseUnitEditViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var courseUnit = await _context.CourseUnits
                .Include(cu => cu.ProgrammeCourseUnits)
                .Include(cu => cu.SubjectCombinationCourseUnits)
                .FirstOrDefaultAsync(cu => cu.CourseUnitId == id);

            if (courseUnit == null)
            {
                return NotFound();
            }

            // Update basic info
            courseUnit.Name = viewModel.Name;
            courseUnit.Code = viewModel.Code;

            // Clear existing programme-course links
            _context.ProgrammeCourseUnits.RemoveRange(courseUnit.ProgrammeCourseUnits);
            courseUnit.ProgrammeCourseUnits.Clear();

            foreach (var prog in viewModel.ProgrammesWithYears)
            {
                foreach (var year in prog.Years.Where(y => y.IsSelected))
                {
                    courseUnit.ProgrammeCourseUnits.Add(new ProgrammeCourseUnit
                    {
                        ProgrammeId = prog.ProgrammeId,
                        YearId = year.YearId
                    });
                }
            }

            // Clear existing subject combination links
            _context.SubjectCombinationCourseUnits.RemoveRange(courseUnit.SubjectCombinationCourseUnits);
            courseUnit.SubjectCombinationCourseUnits.Clear();

            // Add from Education Groups
            foreach (var group in viewModel.EducationGroups)
            {
                foreach (var combo in group.Combinations.Where(c => c.IsSelected))
                {
                    courseUnit.SubjectCombinationCourseUnits.Add(new SubjectCombinationCourseUnit
                    {
                        SubjectCombinationId = combo.SubjectCombinationId,
                        YearId = group.YearId
                    });
                }
            }

            // Add from HEAC group if exists
            if (viewModel.HEACGroup != null)
            {
                foreach (var combo in viewModel.HEACGroup.Combinations.Where(c => c.IsSelected))
                {
                    courseUnit.SubjectCombinationCourseUnits.Add(new SubjectCombinationCourseUnit
                    {
                        SubjectCombinationId = combo.SubjectCombinationId,
                        YearId = _context.Programmes
                            .Where(p => p.ProgrammeId == viewModel.HEACGroup.ProgrammeId)
                            .Select(p => p.YearId)
                            .FirstOrDefault()
                    });
                }
            }

            await _context.SaveChangesAsync();

            TempData["ToastMessage"] = "Course Unit updated successfully!";
            TempData["ToastType"] = "warning";

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int id)
        {
            var courseUnit = await _context.CourseUnits
                .Include(cu => cu.ProgrammeCourseUnits)
                    .ThenInclude(pcu => pcu.Programme)
                .Include(cu => cu.ProgrammeCourseUnits)
                    .ThenInclude(pcu => pcu.Year) // ✅ Include Year
                .Include(cu => cu.SubjectCombinationCourseUnits)
                    .ThenInclude(sccu => sccu.SubjectCombination)
                .Include(cu => cu.SubjectCombinationCourseUnits)
                    .ThenInclude(sccu => sccu.Year) // ✅ Include Year for SubjectCombos too
                .FirstOrDefaultAsync(cu => cu.CourseUnitId == id);

            if (courseUnit == null)
                return NotFound();

            return View(courseUnit);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var courseUnit = await _context.CourseUnits
                .Include(cu => cu.ProgrammeCourseUnits)
                .Include(cu => cu.SubjectCombinationCourseUnits)
                .FirstOrDefaultAsync(cu => cu.CourseUnitId == id);

            if (courseUnit == null)
                return NotFound();

            _context.ProgrammeCourseUnits.RemoveRange(courseUnit.ProgrammeCourseUnits);
            _context.SubjectCombinationCourseUnits.RemoveRange(courseUnit.SubjectCombinationCourseUnits);
            _context.CourseUnits.Remove(courseUnit);

            await _context.SaveChangesAsync();

            TempData["ToastMessage"] = "Course Unit deleted successfully!";
            TempData["ToastType"] = "danger";

            return RedirectToAction(nameof(Index));
        }

    }
}
