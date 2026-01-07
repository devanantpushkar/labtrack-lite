using System.ComponentModel.DataAnnotations;

namespace LabTrackApi.Models
{
    public enum AssetStatus
    {
        Available,
        InUse,
        Maintenance,
        Retired
    }

    public class Asset
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? QRCode { get; set; }

        [Required]
        public AssetStatus Status { get; set; } = AssetStatus.Available;

        [StringLength(100)]
        public string? Location { get; set; }

        [StringLength(50)]
        public string? Category { get; set; }

        public int CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public User? Creator { get; set; }
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}
