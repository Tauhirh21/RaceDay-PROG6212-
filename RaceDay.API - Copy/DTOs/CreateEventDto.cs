using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs
{
    public class CreateEventDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        [MaxLength(200)]
        public string Location { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Province { get; set; }

        [Required]
        [MaxLength(20)]
        public string EventType { get; set; } = string.Empty; // "Run", "Walk", or "Cycle"
    }
}
