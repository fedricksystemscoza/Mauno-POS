using System.ComponentModel.DataAnnotations;

namespace MaunoWeb.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required, MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "Client"; // Admin | SupportStaff | Client

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ── Security fields ──
        public int FailedLoginAttempts { get; set; } = 0;
        public DateTime? LockoutEnd { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public string? LastLoginIp { get; set; }

        // Navigation
        public Client? Client { get; set; }
    }
}
