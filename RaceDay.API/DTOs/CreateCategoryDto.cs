using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs
{
    public class CreateCategoryDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public decimal DistanceKm { get; set; }

        public decimal EntryFee { get; set; }

        public int MaxParticipants { get; set; }
    }
}
