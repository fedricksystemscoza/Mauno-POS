using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Web;
using MaunoWeb.Data;
using MaunoWeb.Models;
using Microsoft.Extensions.Logging;

namespace MaunoWeb.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _db;
        private readonly ILogger<AuthController> _logger;

        // Max failed attempts before lockout
        private const int MaxFailedAttempts = 5;
        private const int LockoutMinutes    = 15;

        public AuthController(AppDbContext db, ILogger<AuthController> logger)
        {
            _db     = db;
            _logger = logger;
        }


        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string Username, string Password)
        {
            // ── Sanitize inputs ──
            Username = HttpUtility.HtmlEncode(Username?.Trim() ?? "");

            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ViewBag.ErrorMessage = "Please enter your username and password.";
                return View();
            }

            var user = _db.Users.FirstOrDefault(u => u.Username == Username);

            // ── Check if user exists ──
            if (user == null)
            {
                _logger.LogWarning("Failed login: unknown username '{Username}' from IP {IP}",
                    Username, HttpContext.Connection.RemoteIpAddress);
                ViewBag.ErrorMessage = "Incorrect username or password.";
                return View();
            }

            // ── Check lockout ──
            if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
            {
                var remaining = (int)(user.LockoutEnd.Value - DateTime.UtcNow).TotalMinutes + 1;
                _logger.LogWarning("Locked account login attempt: '{Username}'", Username);
                ViewBag.ErrorMessage = $"Account is locked. Try again in {remaining} minute(s).";
                return View();
            }

            // ── Verify password ──
            if (!BCrypt.Net.BCrypt.Verify(Password, user.PasswordHash))
            {
                user.FailedLoginAttempts++;

                // Lock account after max attempts
                if (user.FailedLoginAttempts >= MaxFailedAttempts)
                {
                    user.LockoutEnd = DateTime.UtcNow.AddMinutes(LockoutMinutes);
                    _db.SaveChanges();
                    _logger.LogWarning("Account locked after {Attempts} failed attempts: '{Username}'",
                        user.FailedLoginAttempts, Username);
                    ViewBag.ErrorMessage = $"Too many failed attempts. Account locked for {LockoutMinutes} minutes.";
                    return View();
                }

                _db.SaveChanges();
                var attemptsLeft = MaxFailedAttempts - user.FailedLoginAttempts;
                _logger.LogWarning("Failed login for '{Username}'. {Left} attempt(s) remaining.", Username, attemptsLeft);
                ViewBag.ErrorMessage = $"Incorrect password. {attemptsLeft} attempt(s) remaining before lockout.";
                return View();
            }

            // ── Successful login — reset failed attempts ──
            user.FailedLoginAttempts = 0;
            user.LockoutEnd          = null;
            user.LastLoginAt         = DateTime.UtcNow;
            user.LastLoginIp         = HttpContext.Connection.RemoteIpAddress?.ToString();
            _db.SaveChanges();

            _logger.LogInformation("Successful login: '{Username}' Role={Role}", Username, user.Role);

            // ── Create claims ──
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name,           user.Username),
                new Claim(ClaimTypes.Email,          user.Email),
                new Claim(ClaimTypes.Role,           user.Role)
            };

            var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            // ── Role-based redirect ──
            return user.Role switch
            {
                "Admin"        => RedirectToAction("Index", "Dashboard"),
                "SupportStaff" => RedirectToAction("Index", "Dashboard"),
                _              => RedirectToAction("Index", "ClientPortal")
            };
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            _logger.LogInformation("User '{Name}' logged out.", User.Identity?.Name);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(string Username, string Email, string Password, string ConfirmPassword, string Role)
        {
            // ── Sanitize inputs ──
            Username = HttpUtility.HtmlEncode(Username?.Trim() ?? "");
            Email    = HttpUtility.HtmlEncode(Email?.Trim() ?? "");

            // ── Validate required fields ──
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(Role))
            {
                ViewBag.ErrorMessage = "All fields are required.";
                return View();
            }

            // ── Validate email format ──
            if (!Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ViewBag.ErrorMessage = "Please enter a valid email address.";
                return View();
            }

            // ── Password strength ──
            // Min 8 chars, at least 1 uppercase, 1 digit, 1 special character
            if (!Regex.IsMatch(Password, @"^(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$"))
            {
                ViewBag.ErrorMessage = "Password must be at least 8 characters and include an uppercase letter, a number, and a special character (e.g. Useractive@2026).";
                return View();
            }

            // ── Password match ──
            if (Password != ConfirmPassword)
            {
                ViewBag.ErrorMessage = "Passwords do not match.";
                return View();
            }

            // ── Username taken ──
            if (_db.Users.Any(u => u.Username == Username))
            {
                ViewBag.ErrorMessage = "Username is already taken.";
                return View();
            }

            // ── Email taken ──
            if (_db.Users.Any(u => u.Email == Email))
            {
                ViewBag.ErrorMessage = "An account with this email already exists.";
                return View();
            }

            // ── Create user ──
            var user = new User
            {
                Username     = Username,
                Email        = Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(Password),
                Role         = Role,
                CreatedAt    = DateTime.UtcNow
            };

            _db.Users.Add(user);
            _db.SaveChanges();

            // ── Auto-create Client record if role is Client ──
            if (Role == "Client")
            {
                var count  = _db.Clients.Count() + 1;
                var client = new Client
                {
                    UserId         = user.UserId,
                    ClientCode     = $"C{count:D3}",
                    CompanyName    = Username,
                    OwnerName      = Username,
                    Email          = Email,
                    DateRegistered = DateTime.UtcNow,
                    Status         = "Active"
                };
                _db.Clients.Add(client);
                _db.SaveChanges();
            }

            _logger.LogInformation("New account registered: '{Username}' Role={Role}", Username, Role);
            TempData["RegisterSuccess"] = $"Account '{Username}' created successfully! You can now log in.";
            return RedirectToAction("Login");
        }


        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ForgotPassword(string Username, string Email)
        {
            // Sanitize
            Username = HttpUtility.HtmlEncode(Username?.Trim() ?? "");
            Email    = HttpUtility.HtmlEncode(Email?.Trim() ?? "");

            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Email))
            {
                ViewBag.ErrorMessage = "Please enter your username and email.";
                return View();
            }

            // Log the request
            _logger.LogInformation("Password reset requested for username: '{Username}'", Username);

            // Always show success — don't reveal if user exists (security best practice)
            ViewBag.Success = true;
            return View();
        }
    }
}
