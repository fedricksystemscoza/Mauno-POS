using System.ComponentModel.DataAnnotations;

namespace MaunoWeb.Models
{
    public class SoftwareUpdate
    {
        [Key]
        public int UpdateId { get; set; }  // ← make sure this exists

        [Required, MaxLength(20)]
        public string VersionNumber { get; set; } = string.Empty;

        [MaxLength(300)]
        public string Description { get; set; } = string.Empty;

        public DateTime ReleaseDate { get; set; } = DateTime.UtcNow;

        [MaxLength(500)]
        public string FilePath { get; set; } = string.Empty;

        public string FileType { get; set; } = "Installer";

        public bool IsActive { get; set; } = true;
    }
}