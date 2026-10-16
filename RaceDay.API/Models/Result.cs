
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    [Table("Result")]
    public class Result
    {
        [Key]
        public int resultID { get; set; }
        public int registrationID { get; set; }
        public int participantID { get; set; }
        public int eventID { get; set; }
        public int categoryID { get; set; }

        public TimeSpan finish_time { get; set; }
        public int position { get; set; }

        [Required, MaxLength(500)]
        public string split_times { get; set; } = string.Empty;

        public DateTime created_at { get; set; }
    }
}
