using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using MaunoWeb.Data;

namespace MaunoWeb.Controllers
{
    [Authorize(Roles = "Client")]
    public class ClientPortalController : Controller
    {
        private readonly AppDbContext _db;

        public ClientPortalController(AppDbContext db)
        {
            _db = db;
        }

        private int GetClientId()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var client = _db.Clients.FirstOrDefault(c => c.UserId == userId);
            return client?.ClientId ?? 0;
        }

        // GET /ClientPortal
        public IActionResult Index()
        {
            var clientId = GetClientId();
            if (clientId == 0) return RedirectToAction("Login", "Auth");

            var client = _db.Clients
                .Include(c => c.Branches).ThenInclude(b => b.Devices)
                .Include(c => c.Subscriptions)
                .Include(c => c.SupportTickets)
                .Include(c => c.Payments)
                .FirstOrDefault(c => c.ClientId == clientId);

            ViewBag.ActiveSubscription  = client?.Subscriptions.FirstOrDefault(s => s.Status == "Active");
            ViewBag.TotalDevices        = client?.Branches.Sum(b => b.Devices.Count) ?? 0;
            ViewBag.OpenTickets         = client?.SupportTickets.Count(t => t.Status == "Open" || t.Status == "InProgress") ?? 0;
            ViewBag.UnpaidInvoices      = client?.Payments.Count(p => p.Status == "Unpaid" || p.Status == "Overdue") ?? 0;

            return View(client);
        }

        // GET /ClientPortal/MyDevices
        public IActionResult MyDevices()
        {
            var clientId = GetClientId();
            var branches = _db.Branches
                .Include(b => b.Devices)
                .Where(b => b.ClientId == clientId)
                .ToList();
            return View(branches);
        }

        // GET /ClientPortal/MyTickets
        public IActionResult MyTickets()
        {
            var clientId = GetClientId();
            var tickets = _db.SupportTickets
                .Include(t => t.AssignedToUser)
                .Where(t => t.ClientId == clientId)
                .OrderByDescending(t => t.CreatedDate)
                .ToList();
            return View(tickets);
        }

        // POST /ClientPortal/SubmitTicket
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult SubmitTicket(string Subject, string Description, string Priority)
        {
            var clientId = GetClientId();
            if (clientId == 0) return RedirectToAction("Login", "Auth");

            var ticket = new MaunoWeb.Models.SupportTicket
            {
                ClientId    = clientId,
                Subject     = Subject,
                Description = Description,
                Priority    = Priority,
                Status      = "Open",
                CreatedDate = DateTime.UtcNow
            };

            _db.SupportTickets.Add(ticket);
            _db.SaveChanges();

            TempData["Success"] = $"Ticket #{ticket.TicketId} submitted successfully.";
            return RedirectToAction(nameof(MyTickets));
        }

        // GET /ClientPortal/MySubscription
        public IActionResult MySubscription()
        {
            var clientId = GetClientId();
            var subs = _db.Subscriptions
                .Where(s => s.ClientId == clientId)
                .OrderByDescending(s => s.StartDate)
                .ToList();
            return View(subs);
        }

        // GET /ClientPortal/Invoices
        public IActionResult Invoices()
        {
            var clientId = GetClientId();
            var payments = _db.Payments
                .Where(p => p.ClientId == clientId)
                .OrderByDescending(p => p.PaymentDate)
                .ToList();
            return View(payments);
        }

        // GET /ClientPortal/Downloads
        public IActionResult Downloads()
        {
            var updates = _db.SoftwareUpdates
                .Where(u => u.IsActive)
                .OrderByDescending(u => u.ReleaseDate)
                .ToList();
            return View(updates);
        }
    }
}
