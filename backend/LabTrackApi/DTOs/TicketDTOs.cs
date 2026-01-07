using System.ComponentModel.DataAnnotations;
using LabTrackApi.Models;

namespace LabTrackApi.DTOs
{
    public class CreateTicketRequest
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        public TicketPriority Priority { get; set; } = TicketPriority.Medium;

        public int? AssetId { get; set; }
    }

    public class UpdateTicketRequest
    {
        [StringLength(200)]
        public string? Title { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        public TicketStatus? Status { get; set; }

        public TicketPriority? Priority { get; set; }

        public int? AssignedTo { get; set; }
    }

    public class TicketResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public int? AssetId { get; set; }
        public string? AssetName { get; set; }
        public int CreatedBy { get; set; }
        public string? CreatorName { get; set; }
        public int? AssignedTo { get; set; }
        public string? AssigneeName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<CommentResponse> Comments { get; set; } = new List<CommentResponse>();
    }

    public class CreateCommentRequest
    {
        [Required]
        [StringLength(1000)]
        public string Content { get; set; } = string.Empty;
    }

    public class CommentResponse
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public int UserId { get; set; }
        public string? Username { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
