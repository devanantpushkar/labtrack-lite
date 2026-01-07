using System.ComponentModel.DataAnnotations;
using LabTrackApi.Models;

namespace LabTrackApi.DTOs
{
    public class CreateAssetRequest
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? QRCode { get; set; }

        public AssetStatus Status { get; set; } = AssetStatus.Available;

        [StringLength(100)]
        public string? Location { get; set; }

        [StringLength(50)]
        public string? Category { get; set; }
    }

    public class UpdateAssetRequest
    {
        [StringLength(100)]
        public string? Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? QRCode { get; set; }

        public AssetStatus? Status { get; set; }

        [StringLength(100)]
        public string? Location { get; set; }

        [StringLength(50)]
        public string? Category { get; set; }
    }

    public class AssetResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? QRCode { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string? Category { get; set; }
        public int CreatedBy { get; set; }
        public string? CreatorName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
