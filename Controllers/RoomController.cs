using ExamTimeTable.Data;
using ExamTimeTable.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExamTimeTable.Controllers
{
    [Authorize(Roles = "SystemAdmin")]
    public class RoomController : Controller
    {
        private readonly ApplicationDbContext _context;
        public RoomController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            var rooms = from r in _context.Rooms
                              select r;

            if (!string.IsNullOrEmpty(searchString))
            {
                rooms = rooms.Where(d => d.Code.Contains(searchString));
            }

            return View(await rooms.ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RoomId, Code, Block, Capacity")] Room room)
        {
            if (ModelState.IsValid)
            {
                _context.Rooms.Add(room);
                await _context.SaveChangesAsync();
                TempData["ToastMessage"] = "Room added successfully!";
                TempData["ToastType"] = "success";
                return RedirectToAction("Index");
            }
            return View(room);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var room = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomId == id);
            if (room == null)
            {
                return NotFound();
            }
            return View(room);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RoomId, Code, Block, Capacity")] Room room)
        {
            if (id != room.RoomId)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                
                    _context.Update(room);
                    await _context.SaveChangesAsync();
                    TempData["ToastMessage"] = "Room updated successfully!";
                    TempData["ToastType"] = "warning";
                return RedirectToAction("Index");
                
            }
            return View(room);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var room = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomId == id);
            if (room == null)
            {
                return NotFound();
            }
            return View(room);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room != null)
            {
                _context.Rooms.Remove(room);
                await _context.SaveChangesAsync();
                TempData["ToastMessage"] = "Room deleted successfully!";
                TempData["ToastType"] = "danger";
            }
            return RedirectToAction("Index");
        }
    }
}
