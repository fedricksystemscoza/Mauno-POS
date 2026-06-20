using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MaunoWeb.Data;
using MaunoWeb.Models;

namespace MaunoWeb.Controllers
{
    [Authorize(Roles = "Admin,SupportStaff")]
    public class ClientsController : Controller
    {
        private readonly AppDbContext _db;

        public ClientsController(AppDbContext db)
        {
            _db = db;
        }

        // GET /Clients
        public IActionResult Index(string? search, string? status)
        {
            var query = _db.Clients.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(c => c.CompanyName.Contains(search) || c.OwnerName.Contains(search));

            if (!string.IsNullOrEmpty(status))
                query = query.Where(c => c.Status == status);

            ViewBag.Search = search;
            ViewBag.Status = status;

            return View(query.OrderByDescending(c => c.DateRegistered).ToList());
        }

        // GET /Clients/Details/5
        public IActionResult Details(int id)
        {
            var client = _db.Clients
                .Include(c => c.Branches).ThenInclude(b => b.Devices)
                .Include(c => c.Subscriptions)
                .Include(c => c.SupportTickets)
                .Include(c => c.Payments)
                .FirstOrDefault(c => c.ClientId == id);

            if (client == null) return NotFound();
            return View(client);
        }

        // GET /Clients/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create() => View();

        // POST /Clients/Create
        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
        public IActionResult Create(Client client)
        {
            if (!ModelState.IsValid) return View(client);

            // Auto-generate client code
            var count = _db.Clients.Count() + 1;
            client.ClientCode = $"C{count:D3}";
            client.DateRegistered = DateTime.UtcNow;

            _db.Clients.Add(client);
            _db.SaveChanges();

            TempData["Success"] = $"Client {client.CompanyName} created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET /Clients/Edit/5
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int id)
        {
            var client = _db.Clients.Find(id);
            if (client == null) return NotFound();
            return View(client);
        }

        // POST /Clients/Edit/5
        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
        public IActionResult Edit(int id, Client client)
        {
            if (id != client.ClientId) return BadRequest();
            if (!ModelState.IsValid) return View(client);

            _db.Clients.Update(client);
            _db.SaveChanges();

            TempData["Success"] = "Client updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // POST /Clients/Delete/5
        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var client = _db.Clients.Find(id);
            if (client == null) return NotFound();

            _db.Clients.Remove(client);
            _db.SaveChanges();

            TempData["Success"] = "Client removed.";
            return RedirectToAction(nameof(Index));
        }
    }
}