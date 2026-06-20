using System.ComponentModel.DataAnnotations;

namespace MaunoWeb.Models
{
    public class Client
    {
       [Key] public int ClientId { get; set; }

        [Required, MaxLength(20)]
        public string ClientCode { get; set; } = string.Empty; // e.g. C001

        [Required, MaxLength(150)]
        public string CompanyName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string OwnerName { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty;

        [MaxLength(250)]
        public string Address { get; set; } = string.Empty;

        public DateTime DateRegistered { get; set; } = DateTime.UtcNow;

        public string Status { get; set; } = "Active"; // Active | Inactive

        // Foreign key to User
        public int? UserId { get; set; }
        public User? User { get; set; }

        // Navigation
        public ICollection<Branch> Branches { get; set; } = new List<Branch>();
        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
        public ICollection<SupportTicket> SupportTickets { get; set; } = new List<SupportTicket>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
