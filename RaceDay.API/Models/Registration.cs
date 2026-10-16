
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    [Table("Registration")]
    public class Registration
    {
        [Key]
        public int registrationID { get; set; }
        public int participantID { get; set; }
        public int eventID { get; set; }
        public int categoryID { get; set; }

        public DateTime registrationdate { get; set; }

        [Required, MaxLength(50)]
        public string status { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string paymentstatus { get; set; } = string.Empty;

        public int entryfee { get; set; }
        public DateTime created_at { get; set; }
    }
}

