using System.ComponentModel.DataAnnotations;

namespace MaunoWeb.Models
{
    public class Branch
    {
       [Key] public int BranchId { get; set; }

        [Required]
        public int ClientId { get; set; }
        public Client? Client { get; set; }

        [Required, MaxLength(150)]
        public string BranchName { get; set; } = string.Empty;

        [MaxLength(250)]
        public string Address { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Manager { get; set; } = string.Empty;

        public int NumberOfTerminals { get; set; } = 1;

        // Navigation
        public ICollection<Device> Devices { get; set; } = new List<Device>();
    }
}
