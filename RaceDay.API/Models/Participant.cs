
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
//for Participants 
    [Table("Participant")]
    public class Participant
    {
        [Key]
        public int participantID { get; set; }

        [Required, MaxLength(50)]
        public string name { get; set; } = string.Empty;
    //for email 
        [Required, EmailAddress, MaxLength(50)]
        public string email { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string phone { get; set; } = string.Empty;
// for dateofbirth&time
        public DateTime dateofbirth { get; set; }

        [Required, MaxLength(50)]
        public string gender { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string nationality { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string emergency_contact { get; set; } = string.Empty;
      
        public DateTime created_at { get; set; }

        public string? password_hash { get; set; }
    }
}
