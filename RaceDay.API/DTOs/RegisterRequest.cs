
namespace RaceDay.API.DTOs
{
    public class RegisterRequest
    {
        public string Role { get; set; } = "";
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Password { get; set; } = "";

        public string? OrganisationName { get; set; }

        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Nationality { get; set; }
        public string? EmergencyContact { get; set; }
    }
}
