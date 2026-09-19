using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    public class WeatherLog
    {
        [Key]
        public int WeatherLogId { get; set; }

        public int EventId { get; set; }

        [ForeignKey("EventId")]
        public Event? Event { get; set; }

        [Required]
        public DateTime LogDate { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? TemperatureC { get; set; }

        [MaxLength(50)]
        public string? Condition { get; set; }

        public int? Humidity { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? WindSpeedKmh { get; set; }
    }
}