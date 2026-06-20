using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MaunoWeb.Data;
using MaunoWeb.Models;
using System.Security.Claims;

namespace MaunoWeb.Controllers
{
    [Authorize]
    public class SupportTicketsController : Controller
    {
        private readonly AppDbContext _db;

        public SupportTicketsController(AppDbContext db)
        {
            _db = db;
        }

        // GET /SupportTickets
        public IActionResult Index(string? status, string? priority)
        {
            var query = _db.SupportTickets
                .Include(t => t.Client)
                .Include(t => t.AssignedToUser)
                .AsQueryable();

            // Clients only see their own tickets
            if (User.IsInRole("Client"))
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var client = _db.Clients.FirstOrDefault(c => c.UserId == userId);
                if (client != null) query = query.Where(t => t.ClientId == client.ClientId);
            }

            if (!string.IsNullOrEmpty(status)) query = query.Where(t => t.Status == status);
            if (!string.IsNullOrEmpty(priority)) query = query.Where(t => t.Priority == priority);

            ViewBag.Status = status;
            ViewBag.Priority = priority;

            return View(query.OrderByDescending(t => t.CreatedDate).ToList());
        }

        // GET /SupportTickets/Create
        public IActionResult Create() => View();

        // POST /SupportTickets/Create
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Create(SupportTicket ticket)
        {
            if (!ModelState.IsValid) return View(ticket);

            ticket.CreatedDate = DateTime.UtcNow;
            ticket.Status = "Open";

            _db.SupportTickets.Add(ticket);
            _db.SaveChanges();

            TempData["Success"] = $"Ticket #{ticket.TicketId} created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET /SupportTickets/Details/5
        public IActionResult Details(int id)
        {
            var ticket = _db.SupportTickets
                .Include(t => t.Client)
                .Include(t => t.AssignedToUser)
                .FirstOrDefault(t => t.TicketId == id);

            if (ticket == null) return NotFound();
            return View(ticket);
        }

        // POST /SupportTickets/UpdateStatus
        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin,SupportStaff")]
        public IActionResult UpdateStatus(int id, string status, int? assignedToUserId)
        {
            var ticket = _db.SupportTickets.Find(id);
            if (ticket == null) return NotFound();

            ticket.Status = status;
            if (assignedToUserId.HasValue) ticket.AssignedToUserId = assignedToUserId;
            if (status == "Resolved") ticket.ResolvedDate = DateTime.UtcNow;

            _db.SaveChanges();

            TempData["Success"] = $"Ticket #{id} updated to {status}.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
