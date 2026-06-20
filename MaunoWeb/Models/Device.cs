using System.ComponentModel.DataAnnotations;

namespace MaunoWeb.Models
{
    public class Device
    {
       [Key] public int DeviceId { get; set; }

        [Required]
        public int BranchId { get; set; }
        public Branch? Branch { get; set; }

        [Required, MaxLength(50)]
        public string SerialNumber { get; set; } = string.Empty; // e.g. POS001

        [MaxLength(100)]
        public string DeviceType { get; set; } = string.Empty;

        [MaxLength(20)]
        public string VersionInstalled { get; set; } = string.Empty; // e.g. v3.2

        public DateTime InstallationDate { get; set; } = DateTime.UtcNow;

        public string Status { get; set; } = "Active"; // Active | Inactive | Faulty
    }
}
