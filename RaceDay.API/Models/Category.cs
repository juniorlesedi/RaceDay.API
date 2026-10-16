
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    [Table("Category")]
    public class Category
    {
        [Key]
        public int categoryID { get; set; }
        public int eventID { get; set; }

        [Required, MaxLength(50)]
        public string name { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string agegroup { get; set; } = string.Empty;

        [Required, MaxLength(10)]
        public string gender { get; set; } = string.Empty;

        [Column(TypeName = "decimal(5,2)")]
        public decimal distancekm { get; set; }

        public int max_participants { get; set; }
    }
}

