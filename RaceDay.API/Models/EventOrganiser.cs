
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    [Table("Event_Organiser")]
    public class EventOrganiser
    {
        [Key]
        public int organiserID { get; set; }

        [Required, MaxLength(50)]
        public string name { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(50)]
        public string email { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string phone { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string organisation_name { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string user_type { get; set; } = "Organiser";

        [Required, MaxLength(255)]
        public string password_hash { get; set; } = string.Empty;

        public DateTime created_at { get; set; }
    }
}

