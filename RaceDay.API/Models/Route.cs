
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    [Table("Route")]
    public class Route
    {
        [Key]
        public int routeID { get; set; }
        public int eventID { get; set; }
        public int categoryID { get; set; }

        [Column(TypeName = "decimal(6,2)")]
        public decimal elevation_gain { get; set; }

        [Required, MaxLength(255)]
        public string map_url { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string description { get; set; } = string.Empty;

        public DateTime created_at { get; set; }
    }
}
