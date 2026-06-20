using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MaunoWeb.Data;

namespace MaunoWeb.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _db;

        public DashboardController(AppDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            ViewBag.TotalClients = _db.Clients.Count();
            ViewBag.ActiveSubscriptions = _db.Subscriptions.Count(s => s.Status == "Active");
            ViewBag.OpenTickets = _db.SupportTickets.Count(t => t.Status == "Open" || t.Status == "InProgress");
            ViewBag.MonthlyRevenue = _db.Payments
                                            .Where(p => p.Status == "Paid" && p.PaymentDate.Month == DateTime.UtcNow.Month)
                                            .Sum(p => (decimal?)p.Amount) ?? 0;

            return View();
        }
    }
}
