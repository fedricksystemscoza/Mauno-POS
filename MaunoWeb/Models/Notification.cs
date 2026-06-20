using System.ComponentModel.DataAnnotations;

namespace MaunoWeb.Models
{
    public class Notification
    {
        [Key]public int NotificationId { get; set; }

        [Required]
        public int UserId { get; set; }
        public User? User { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        public string Type { get; set; } = "Info"; // Info | Warning | Alert

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}