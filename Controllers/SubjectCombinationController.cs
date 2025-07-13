using ExamTimeTable.Data;
using ExamTimeTable.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ExamTimeTable.Controllers
{
    public class SubjectCombinationController : Controller
    {
        private readonly ApplicationDbContext _context;
        public SubjectCombinationController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(string searchString)
        {
            var combinations = _context.SubjectCombinations
                .Include(p => p.Programme)
                .Include(y => y.Programme.Year)
                .Include(ds => ds.DepartmentSubjectCombinations)
                .ThenInclude(d => d.Department)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                combinations = combinations.Where(c => c.Name.Contains(searchString));
            }

            return View(await combinations.ToListAsync());
        }

        public IActionResult Create()
        {
            var programmes = _context.Programmes
                .Include(p => p.Year)
                .Where(p => p.NumberOfStudents == null)
                .Select(p => new
                {
                    ProgrammeId = p.ProgrammeId,
                    DisplayName = p.Name + " - " + (p.Year != null ? p.Year.Name : "No Year")
                })
                .ToList();

            ViewBag.Programmes = new SelectList(programmes, "ProgrammeId", "DisplayName");
            ViewBag.Departments = new MultiSelectList(_context.Departments.ToList(), "DepartmentId", "Name");

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubjectCombination combination)
        {
            if (ModelState.IsValid)
            {
                _context.SubjectCombinations.Add(combination);
                await _context.SaveChangesAsync();
                TempData["ToastMessage"] = "Subject Combination added successfully!";
                TempData["ToastType"] = "success";

                if (combination.SelectedDepartmentIds != null)
                {
                    foreach (var departmentId in combination.SelectedDepartmentIds)
                    {
                        _context.DepartmentSubjectCombinations.Add(new DepartmentSubjectCombination
                        {
                            DepartmentId = departmentId,
                            SubjectCombinationId = combination.SubjectCombinationId
                        });
                    }
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction("Index");
            }

            return View(combination);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var combination = await _context.SubjectCombinations
                .Include(sc => sc.DepartmentSubjectCombinations) // Make sure you include the related data
                .FirstOrDefaultAsync(c => c.SubjectCombinationId == id);

            if (combination == null)
            {
                return NotFound();
            }

            // Prepare the Programme dropdown
            var programmes = _context.Programmes
                .Include(p => p.Year)
                .Where(p => p.NumberOfStudents == null)
                .Select(p => new
                {
                    ProgrammeId = p.ProgrammeId,
                    DisplayName = p.Name + " - " + (p.Year != null ? p.Year.Name : "No Year")
                })
                .ToList();

            ViewBag.Programmes = new SelectList(programmes, "ProgrammeId", "DisplayName");

            // Extract selected department IDs
            var selectedDepartmentIds = combination.DepartmentSubjectCombinations
                .Select(ds => ds.DepartmentId)
                .ToList();

            // Assign to model
            combination.SelectedDepartmentIds = selectedDepartmentIds;

            // Build the MultiSelectList with selected values
            ViewBag.Departments = new MultiSelectList(_context.Departments.ToList(), "DepartmentId", "Name", selectedDepartmentIds);

            return View(combination);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SubjectCombination combination)
        {
            if (id != combination.SubjectCombinationId)
                return NotFound();

            if (!ModelState.IsValid)
                return View(combination);

            // 1. Get the existing combination with its departments
            var existingCombination = await _context.SubjectCombinations
                .Include(sc => sc.DepartmentSubjectCombinations)
                .FirstOrDefaultAsync(sc => sc.SubjectCombinationId == id);

            if (existingCombination == null)
                return NotFound();

            // 2. Update basic fields
            existingCombination.Name = combination.Name;
            existingCombination.NumberOfStudents = combination.NumberOfStudents;
            existingCombination.ProgrammeId = combination.ProgrammeId;

            // 3. Remove existing department links
            _context.DepartmentSubjectCombinations.RemoveRange(existingCombination.DepartmentSubjectCombinations);

            // 4. Add selected departments
            if (combination.SelectedDepartmentIds != null)
            {
                foreach (var departmentId in combination.SelectedDepartmentIds)
                {
                    _context.DepartmentSubjectCombinations.Add(new DepartmentSubjectCombination
                    {
                        DepartmentId = departmentId,
                        SubjectCombinationId = combination.SubjectCombinationId
                    });
                }
            }

            // 5. Save changes
            await _context.SaveChangesAsync();
            TempData["ToastMessage"] = "Subject Combination updated successfully!";
            TempData["ToastType"] = "warning";

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int id)
        {
            var combination = await _context.SubjectCombinations
                .Include(p => p.Programme)
                .Include(y => y.Programme.Year)
                .Include(ds => ds.DepartmentSubjectCombinations)
                .ThenInclude(d => d.Department)
                .FirstOrDefaultAsync(c => c.SubjectCombinationId == id);
            return View(combination);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var combination = await _context.SubjectCombinations
                .Include(sc => sc.DepartmentSubjectCombinations)
                .FirstOrDefaultAsync(sc => sc.SubjectCombinationId == id);

            if (combination != null)
            {
                _context.DepartmentSubjectCombinations.RemoveRange(combination.DepartmentSubjectCombinations);
                _context.SubjectCombinations.Remove(combination);
                await _context.SaveChangesAsync();

                TempData["ToastMessage"] = "Subject Combination deleted successfully!";
                TempData["ToastType"] = "danger";
            }

            return RedirectToAction("Index");
        }

    }
}
