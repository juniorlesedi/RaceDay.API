
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    [Table("Event")]
    public class RaceEvent
    {
        [Key]
        public int eventID { get; set; }
        public int organiserID { get; set; }

        [Required, MaxLength(50)]
        public string name { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string description { get; set; } = string.Empty;

        public DateTime event_date { get; set; }

        [Required, MaxLength(50)]
        public string location { get; set; } = string.Empty;

        [Column(TypeName = "decimal(5,2)")]
        public decimal Distance_km { get; set; }

        [Required, MaxLength(50)]
        public string event_type { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string status { get; set; } = string.Empty;

        public DateTime created_at { get; set; }
    }
}
