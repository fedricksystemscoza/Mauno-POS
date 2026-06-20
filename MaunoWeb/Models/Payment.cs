using System.ComponentModel.DataAnnotations;

namespace MaunoWeb.Models
{
    public class Payment
    {
       [Key] public int PaymentId { get; set; }

        [Required]
        public int ClientId { get; set; }
        public Client? Client { get; set; }

        [Required, MaxLength(50)]
        public string InvoiceNumber { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        public string Status { get; set; } = "Unpaid"; // Paid | Unpaid | Overdue
    }
}
