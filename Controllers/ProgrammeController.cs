using ExamTimeTable.Data;
using ExamTimeTable.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ExamTimeTable.Controllers
{
    [Authorize(Roles = "SystemAdmin, Admin")]
    public class ProgrammeController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ProgrammeController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(string searchString, string sortOrder, int? yearFilter, int? departmentFilter)
        {
            ViewBag.NameSortParam = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.DeptSortParam = sortOrder == "dept_asc" ? "dept_desc" : "dept_asc";
            ViewBag.StudentsSortParam = sortOrder == "students_asc" ? "students_desc" : "students_asc";

            // Load filters
            ViewBag.Years = new SelectList(await _context.Years.OrderBy(y => y.Name).ToListAsync(), "YearId", "Name");
            ViewBag.Departments = new SelectList(await _context.Departments.OrderBy(d => d.Name).ToListAsync(), "DepartmentId", "Name");

            var programmes = _context.Programmes
                .Include(p => p.Department)
                .Include(p => p.Year)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
                programmes = programmes.Where(p => p.Name.Contains(searchString));

            if (yearFilter.HasValue)
            {
                programmes = programmes.Where(p => p.YearId == yearFilter.Value);
                ViewBag.SelectedYear = yearFilter.Value;
            }

            if (departmentFilter.HasValue)
            {
                programmes = programmes.Where(p => p.DepartmentId == departmentFilter.Value);
                ViewBag.SelectedDepartment = departmentFilter.Value;
            }

            programmes = sortOrder switch
            {
                "name_desc" => programmes.OrderByDescending(p => p.Name),
                "dept_asc" => programmes.OrderBy(p => p.Department.Name),
                "dept_desc" => programmes.OrderByDescending(p => p.Department.Name),
                "students_asc" => programmes.OrderBy(p => p.NumberOfStudents),
                "students_desc" => programmes.OrderByDescending(p => p.NumberOfStudents),
                _ => programmes.OrderBy(p => p.Name),
            };

            return View(await programmes.ToListAsync());
        }

        public IActionResult Create()
        {
            ViewBag.Years = new SelectList(_context.Years, "YearId", "Name");
            ViewBag.Departments = new SelectList(_context.Departments, "DepartmentId", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProgrammeId, Name, NumberOfStudents, YearId, DepartmentId")] Programme programme)
        {
            if (ModelState.IsValid)
            {
                _context.Programmes.Add(programme);
                await _context.SaveChangesAsync();
                TempData["ToastMessage"] = "Program added successfully!";
                TempData["ToastType"] = "success";
                return RedirectToAction("Index");
            }

            // Re-populate dropdowns before redisplaying the form
            ViewBag.Years = new SelectList(_context.Years, "YearId", "Name", programme.YearId);
            ViewBag.Departments = new SelectList(_context.Departments, "DepartmentId", "Name", programme.DepartmentId);

            return View(programme);
        }

        public async Task<IActionResult> Edit(int id)
        {
            ViewBag.Years = new SelectList(_context.Years, "YearId", "Name");
            ViewBag.Departments = new SelectList(_context.Departments, "DepartmentId", "Name");
            var programme = await _context.Programmes.FirstOrDefaultAsync(p => p.ProgrammeId == id);
            return View(programme);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProgrammeId, Name, NumberOfStudents, YearId, DepartmentId")] Programme programme)
        {
            if (ModelState.IsValid)
            {
                _context.Update(programme);
                await _context.SaveChangesAsync();
                TempData["ToastMessage"] = "Program updated successfully!";
                TempData["ToastType"] = "warning";
                return RedirectToAction("Index");
            }

            // Re-populate dropdowns before redisplaying the form
            ViewBag.Years = new SelectList(_context.Years, "YearId", "Name", programme.YearId);
            ViewBag.Departments = new SelectList(_context.Departments, "DepartmentId", "Name", programme.DepartmentId);

            return View(programme);
        }

        [Authorize(Roles = "SystemAdmin")]
        public async Task<IActionResult> Delete(int id)
        {
            var programme = await _context.Programmes
                .Include(d => d.Department)
                .Include(y => y.Year)
                .FirstOrDefaultAsync(p => p.ProgrammeId == id);
            return View(programme);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var programme = await _context.Programmes.FindAsync(id);
            if (programme != null)
            {
                _context.Programmes.Remove(programme);
                await _context.SaveChangesAsync();
                TempData["ToastMessage"] = "Program deleted successfully!";
                TempData["ToastType"] = "danger";
            }
            return RedirectToAction("Index");
        }
    }
}
