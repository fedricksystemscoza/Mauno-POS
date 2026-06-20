using System.ComponentModel.DataAnnotations;

namespace MaunoWeb.Models
{
    public class SupportTicket
    {
       [Key] public int TicketId { get; set; }

        [Required]
        public int ClientId { get; set; }
        public Client? Client { get; set; }

        // Staff assigned to this ticket
        public int? AssignedToUserId { get; set; }
        public User? AssignedToUser { get; set; }

        [Required, MaxLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public string Priority { get; set; } = "Medium"; // Low | Medium | High | Critical

        public string Status { get; set; } = "Open"; // Open | Assigned | InProgress | Resolved | Closed

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ResolvedDate { get; set; }
    }
}
