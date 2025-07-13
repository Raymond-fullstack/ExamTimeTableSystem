using ExamTimeTable.Data;
using ExamTimeTable.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExamTimeTable.Controllers
{
    public class DepartmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        public DepartmentController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(string searchString)
        {
            var departments = from d in _context.Departments
                              select d;

            if (!string.IsNullOrEmpty(searchString))
            {
                departments = departments.Where(d => d.Name.Contains(searchString));
            }

            return View(await departments.ToListAsync());
        }


        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DepartmentId, Name, Description")] Department department)
        {
            if (ModelState.IsValid)
            {
                _context.Departments.Add(department);
                await _context.SaveChangesAsync();
                TempData["ToastMessage"] = "Department added successfully!";
                TempData["ToastType"] = "success";
                return RedirectToAction("Index");
            }
            return View(department);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var department = await _context.Departments.FirstOrDefaultAsync(d => d.DepartmentId == id);
            return View(department);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DepartmentId, Name, Description")] Department department)
        {
            if (id != department.DepartmentId)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                _context.Update(department);
                await _context.SaveChangesAsync();
                TempData["ToastMessage"] = "Department updated successfully!";
                TempData["ToastType"] = "warning";
                return RedirectToAction("Index");
            }
            return View(department);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var department = await _context.Departments.FirstOrDefaultAsync(d => d.DepartmentId == id);
            if (department == null)
            {
                return NotFound();
            }
            return View(department);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department != null)
            {
                _context.Departments.Remove(department);
                await _context.SaveChangesAsync();
                TempData["ToastMessage"] = "Department deleted successfully!";
                TempData["ToastType"] = "danger";
            }
            return RedirectToAction("Index");
        }
    }
}
