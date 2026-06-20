using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MaunoWeb.Data;
using MaunoWeb.Models;

namespace MaunoWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SubscriptionsController : Controller
    {
        private readonly AppDbContext _db;

        public SubscriptionsController(AppDbContext db)
        {
            _db = db;
        }

        // GET /Subscriptions
        public IActionResult Index(string? status)
        {
            var query = _db.Subscriptions.Include(s => s.Client).AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(s => s.Status == status);

            // Auto-expire subscriptions past their expiry date
            var expired = query.Where(s => s.ExpiryDate < DateTime.UtcNow && s.Status == "Active").ToList();
            expired.ForEach(s => s.Status = "Expired");
            if (expired.Any()) _db.SaveChanges();

            ViewBag.Status = status;
            return View(query.OrderByDescending(s => s.StartDate).ToList());
        }

        // GET /Subscriptions/Create
        public IActionResult Create()
        {
            ViewBag.Clients = _db.Clients.Where(c => c.Status == "Active").ToList();
            return View();
        }

        // POST /Subscriptions/Create
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Create(Subscription sub)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Clients = _db.Clients.Where(c => c.Status == "Active").ToList();
                return View(sub);
            }

            _db.Subscriptions.Add(sub);
            _db.SaveChanges();

            TempData["Success"] = "Subscription created.";
            return RedirectToAction(nameof(Index));
        }

        // POST /Subscriptions/Suspend/5
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Suspend(int id)
        {
            var sub = _db.Subscriptions.Find(id);
            if (sub == null) return NotFound();
            sub.Status = "Suspended";
            _db.SaveChanges();
            TempData["Success"] = "Subscription suspended.";
            return RedirectToAction(nameof(Index));
        }
    }
}
