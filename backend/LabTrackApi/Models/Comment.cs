using System.ComponentModel.DataAnnotations;

namespace LabTrackApi.Models
{
    public class Comment
    {
        public int Id { get; set; }

        public int TicketId { get; set; }

        public int UserId { get; set; }

        [Required]
        [StringLength(1000)]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Ticket? Ticket { get; set; }
        public User? User { get; set; }
    }
}
