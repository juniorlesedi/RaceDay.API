
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        private readonly PasswordHasher<EventOrganiser> _organiserHasher = new();
        private readonly PasswordHasher<Participant> _participantHasher = new();

        public AuthController(RaceDayDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Phone) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Name, email, phone and password are required." });
            }

            if (request.Password.Length < 8)
                return BadRequest(new { message = "Password must be at least 8 characters." });

            if (request.Role.Equals("Organiser", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(request.OrganisationName))
                    return BadRequest(new { message = "Organisation name is required for Organisers." });
            }
            else if (request.Role.Equals("Participant", StringComparison.OrdinalIgnoreCase))
            {
                if (request.DateOfBirth == null ||
                    string.IsNullOrWhiteSpace(request.Gender) ||
                    string.IsNullOrWhiteSpace(request.Nationality) ||
                    string.IsNullOrWhiteSpace(request.EmergencyContact))
                {
                    return BadRequest(new { message = "Date of birth, gender, nationality and emergency contact are required for Participants." });
                }
            }
            else
            {
                return BadRequest(new { message = "Role must be Organiser or Participant." });
            }

            bool organiserExists = await _context.Event_Organiser
                .AnyAsync(x => x.email == request.Email);

            bool participantExists = await _context.Participant
                .AnyAsync(x => x.email == request.Email);

            if (organiserExists || participantExists)
                return Conflict(new { message = "An account with this email already exists." });

            if (request.Role.Equals("Organiser", StringComparison.OrdinalIgnoreCase))
            {
                var organiser = new EventOrganiser
                {
                    organiserID = (await _context.Event_Organiser
                        .MaxAsync(x => (int?)x.organiserID) ?? 0) + 1,
                    name = request.Name.Trim(),
                    email = request.Email.Trim(),
                    phone = request.Phone.Trim(),
                    organisation_name = request.OrganisationName!.Trim(),
                    user_type = "Organiser",
                    created_at = DateTime.Now
                };

                organiser.password_hash = _organiserHasher.HashPassword(
                    organiser, request.Password);

                _context.Event_Organiser.Add(organiser);
            }
            else
            {
                var participant = new Participant
                {
                    participantID = (await _context.Participant
                        .MaxAsync(x => (int?)x.participantID) ?? 0) + 1,
                    name = request.Name.Trim(),
                    email = request.Email.Trim(),
                    phone = request.Phone.Trim(),
                    dateofbirth = request.DateOfBirth!.Value,
                    gender = request.Gender!.Trim(),
                    nationality = request.Nationality!.Trim(),
                    emergency_contact = request.EmergencyContact!.Trim(),
                    created_at = DateTime.Now
                };

                participant.password_hash = _participantHasher.HashPassword(
                    participant, request.Password);

                _context.Participant.Add(participant);
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Account registered successfully." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Email and password are required." });
            }

            var organiser = await _context.Event_Organiser
                .FirstOrDefaultAsync(x => x.email == request.Email);

            if (organiser != null &&
                _organiserHasher.VerifyHashedPassword(
                    organiser, organiser.password_hash, request.Password)
                != PasswordVerificationResult.Failed)
            {
                HttpContext.Session.Clear();
                HttpContext.Session.SetString("Role", "Organiser");
                HttpContext.Session.SetInt32("UserId", organiser.organiserID);

                return Ok(new
                {
                    message = "Login successful.",
                    role = "Organiser",
                    name = organiser.name
                });
            }

            var participant = await _context.Participant
                .FirstOrDefaultAsync(x => x.email == request.Email);

            if (participant != null &&
                !string.IsNullOrWhiteSpace(participant.password_hash) &&
                _participantHasher.VerifyHashedPassword(
                    participant, participant.password_hash, request.Password)
                != PasswordVerificationResult.Failed)
            {
                HttpContext.Session.Clear();
                HttpContext.Session.SetString("Role", "Participant");
                HttpContext.Session.SetInt32("UserId", participant.participantID);

                return Ok(new
                {
                    message = "Login successful.",
                    role = "Participant",
                    name = participant.name
                });
            }

            return Unauthorized(new { message = "Invalid email or password." });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok(new { message = "Logged out successfully." });
        }

        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            string? role = HttpContext.Session.GetString("Role");
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (role == null || userId == null)
                return Unauthorized(new { message = "Please log in first." });

            if (role == "Organiser")
            {
                var user = await _context.Event_Organiser
                    .FindAsync(userId.Value);

                if (user == null)
                    return Unauthorized();

                return Ok(new
                {
                    user.organiserID,
                    user.name,
                    user.email,
                    role
                });
            }

            if (role == "Participant")
            {
                var user = await _context.Participant
                    .FindAsync(userId.Value);

                if (user == null)
                    return Unauthorized();

                return Ok(new
                {
                    user.participantID,
                    user.name,
                    user.email,
                    role
                });
            }

            return Unauthorized();
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }
}
