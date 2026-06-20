using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MaunoWeb.Data;
using MaunoWeb.Models;

namespace MaunoWeb.Controllers
{
    [Authorize]
    public class SoftwareUpdatesController : Controller
    {
        private readonly AppDbContext _db;

        public SoftwareUpdatesController(AppDbContext db)
        {
            _db = db;
        }

        // GET /SoftwareUpdates — all users can view
        public IActionResult Index()
        {
            var updates = _db.SoftwareUpdates
                .Where(u => u.IsActive)
                .OrderByDescending(u => u.ReleaseDate)
                .ToList();

            return View(updates);
        }

        // GET /SoftwareUpdates/Create — Admin only
        [Authorize(Roles = "Admin")]
        public IActionResult Create() => View();

        // POST /SoftwareUpdates/Create
        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
        public IActionResult Create(SoftwareUpdate update)
        {
            if (!ModelState.IsValid) return View(update);

            update.ReleaseDate = DateTime.UtcNow;
            _db.SoftwareUpdates.Add(update);
            _db.SaveChanges();

            TempData["Success"] = $"Update {update.VersionNumber} published.";
            return RedirectToAction(nameof(Index));
        }

        // POST /SoftwareUpdates/Deactivate/5
        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
        public IActionResult Deactivate(int id)
        {
            var update = _db.SoftwareUpdates.Find(id);
            if (update == null) return NotFound();
            update.IsActive = false;
            _db.SaveChanges();
            TempData["Success"] = "Update deactivated.";
            return RedirectToAction(nameof(Index));
        }
    }
}
