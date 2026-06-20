using System.ComponentModel.DataAnnotations;

namespace MaunoWeb.Models
{
    public class Subscription
    {
        [Key] public int SubscriptionId { get; set; }

        [Required]
        public int ClientId { get; set; }
        public Client? Client { get; set; }

        [Required, MaxLength(100)]
        public string Package { get; set; } = string.Empty; // e.g. Basic | Standard | Premium

        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime ExpiryDate { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        public string Status { get; set; } = "Active"; // Active | Expired | Suspended
    }
}
