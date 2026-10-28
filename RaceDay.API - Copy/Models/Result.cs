using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    public class Result
    {
        [Key]
        public int ResultId { get; set; }

        public int EnrolmentId { get; set; }

        [ForeignKey("EnrolmentId")]
        public Enrolment? Enrolment { get; set; }

        public TimeSpan? FinishTime { get; set; }

        public int? Position { get; set; }

        public bool DidNotFinish { get; set; } = false;

        public DateTime CapturedAt { get; set; } = DateTime.UtcNow;
    }
}