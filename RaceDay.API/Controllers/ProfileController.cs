
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ProfileController(RaceDayDbContext context)
        {
            _context = context;
        }

        // PUT: api/Profile
        [HttpPut]
        public async Task<IActionResult> UpdateProfile(ProfileInput input)
        {
            string? role = HttpContext.Session.GetString("Role");
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null ||
                (role != "Organiser" && role != "Participant"))
            {
                return Unauthorized(new
                {
                    message = "Please log in first."
                });
            }

            if (string.IsNullOrWhiteSpace(input.Name) ||
                string.IsNullOrWhiteSpace(input.Email) ||
                string.IsNullOrWhiteSpace(input.Phone))
            {
                return BadRequest(new
                {
                    message = "Name, email and phone are required."
                });
            }

            if (input.Name.Trim().Length > 50 ||
                input.Email.Trim().Length > 50 ||
                input.Phone.Trim().Length > 50)
            {
                return BadRequest(new
                {
                    message = "Name, email or phone is too long."
                });
            }

            string email = input.Email.Trim();

            bool emailUsedByOrganiser = await _context.Event_Organiser
                .AnyAsync(o => o.email == email &&
                    !(role == "Organiser" && o.organiserID == userId.Value));

            bool emailUsedByParticipant = await _context.Participant
                .AnyAsync(p => p.email == email &&
                    !(role == "Participant" && p.participantID == userId.Value));

            if (emailUsedByOrganiser || emailUsedByParticipant)
            {
                return Conflict(new
                {
                    message = "That email address is already in use."
                });
            }

            if (role == "Organiser")
            {
                var organiser = await _context.Event_Organiser
                    .FirstOrDefaultAsync(o => o.organiserID == userId.Value);

                if (organiser == null)
                    return NotFound(new { message = "Organiser not found." });

                organiser.name = input.Name.Trim();
                organiser.email = email;
                organiser.phone = input.Phone.Trim();

                if (input.OrganisationName != null)
                {
                    if (input.OrganisationName.Trim().Length > 50)
                        return BadRequest(new
                        {
                            message = "Organisation name is too long."
                        });

                    organiser.organisation_name =
                        input.OrganisationName.Trim();
                }
            }
            else
            {
                var participant = await _context.Participant
                    .FirstOrDefaultAsync(p => p.participantID == userId.Value);

                if (participant == null)
                    return NotFound(new { message = "Participant not found." });

                participant.name = input.Name.Trim();
                participant.email = email;
                participant.phone = input.Phone.Trim();

                if (input.DateOfBirth.HasValue)
                    participant.dateofbirth = input.DateOfBirth.Value;

                if (input.Gender != null)
                    participant.gender = input.Gender.Trim();

                if (input.Nationality != null)
                    participant.nationality = input.Nationality.Trim();

                if (input.EmergencyContact != null)
                    participant.emergency_contact =
                        input.EmergencyContact.Trim();
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Profile updated successfully."
            });
        }
    }

    public class ProfileInput
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = "";

        [Required]
        [EmailAddress]
        [StringLength(50)]
        public string Email { get; set; } = "";

        [Required]
        [StringLength(50)]
        public string Phone { get; set; } = "";

        public string? OrganisationName { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string? Gender { get; set; }

        public string? Nationality { get; set; }

        public string? EmergencyContact { get; set; }
    }
}
