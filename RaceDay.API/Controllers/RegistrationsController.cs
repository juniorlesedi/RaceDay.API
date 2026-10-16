
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegistrationsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public RegistrationsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // POST: api/Registrations
        // A participant enrols in an event category.
        [HttpPost]
        public async Task<IActionResult> RegisterForEvent(RegistrationInput input)
        {
            if (HttpContext.Session.GetString("Role") != "Participant")
                return Unauthorized(new
                {
                    message = "Log in as a Participant first."
                });

            int? participantId = HttpContext.Session.GetInt32("UserId");

            if (participantId == null)
                return Unauthorized(new { message = "Please log in again." });

            var category = await _context.Category
                .FirstOrDefaultAsync(c => c.categoryID == input.CategoryId);

            if (category == null)
                return NotFound(new { message = "Category not found." });

            if (category.eventID != input.EventId)
                return BadRequest(new
                {
                    message = "This category does not belong to the selected event."
                });

            var raceEvent = await _context.Event
                .FirstOrDefaultAsync(e => e.eventID == input.EventId);

            if (raceEvent == null)
                return NotFound(new { message = "Event not found." });

            if (raceEvent.event_date <= DateTime.Now)
                return BadRequest(new
                {
                    message = "Registration is closed because the event date has passed or started."
                });

            bool alreadyRegistered = await _context.Registration.AnyAsync(r =>
                r.participantID == participantId.Value &&
                r.eventID == input.EventId);

            if (alreadyRegistered)
                return Conflict(new
                {
                    message = "You are already registered for this event."
                });

            int currentRegistrations = await _context.Registration.CountAsync(r =>
                r.categoryID == input.CategoryId &&
                r.status != "Cancelled");

            if (currentRegistrations >= category.max_participants)
                return Conflict(new
                {
                    message = "This category is full."
                });

            int newId = (await _context.Registration
                .MaxAsync(r => (int?)r.registrationID) ?? 0) + 1;

            var registration = new Registration
            {
                registrationID = newId,
                participantID = participantId.Value,
                eventID = input.EventId,
                categoryID = input.CategoryId,
                registrationdate = DateTime.Now,
                status = "Registered",
                paymentstatus = "Unpaid",
                entryfee = 0,
                created_at = DateTime.Now
            };

            _context.Registration.Add(registration);
            await _context.SaveChangesAsync();

            return Created(
                $"/api/Registrations/{registration.registrationID}",
                registration);
        }

        // GET: api/Registrations/my
        // Participants can see only their own registrations.
        [HttpGet("my")]
        public async Task<IActionResult> GetMyRegistrations()
        {
            if (HttpContext.Session.GetString("Role") != "Participant")
                return Unauthorized(new
                {
                    message = "Log in as a Participant first."
                });

            int? participantId = HttpContext.Session.GetInt32("UserId");

            if (participantId == null)
                return Unauthorized(new { message = "Please log in again." });

            var registrations = await _context.Registration
                .AsNoTracking()
                .Where(r => r.participantID == participantId.Value)
                .OrderByDescending(r => r.registrationdate)
                .ToListAsync();

            return Ok(registrations);
        }

        // GET: api/Registrations/event/1
        // Only the organiser who owns the event can view its registrations.
        [HttpGet("event/{eventId:int}")]
        public async Task<IActionResult> GetEventRegistrations(int eventId)
        {
            if (HttpContext.Session.GetString("Role") != "Organiser")
                return Unauthorized(new
                {
                    message = "Log in as an Organiser first."
                });

            int? organiserId = HttpContext.Session.GetInt32("UserId");

            if (organiserId == null)
                return Unauthorized(new { message = "Please log in again." });

            var raceEvent = await _context.Event
                .FirstOrDefaultAsync(e => e.eventID == eventId);

            if (raceEvent == null)
                return NotFound(new { message = "Event not found." });

            if (raceEvent.organiserID != organiserId.Value)
                return Forbid();

            var registrations = await _context.Registration
                .AsNoTracking()
                .Where(r => r.eventID == eventId)
                .OrderBy(r => r.registrationdate)
                .ToListAsync();

            return Ok(registrations);
        }

        // DELETE: api/Registrations/1
        // Participants can cancel their own registration.
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> CancelRegistration(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Participant")
                return Unauthorized(new
                {
                    message = "Log in as a Participant first."
                });

            int? participantId = HttpContext.Session.GetInt32("UserId");

            if (participantId == null)
                return Unauthorized(new { message = "Please log in again." });

            var registration = await _context.Registration
                .FirstOrDefaultAsync(r => r.registrationID == id);

            if (registration == null)
                return NotFound(new { message = "Registration not found." });

            if (registration.participantID != participantId.Value)
                return Forbid();

            if (registration.status == "Cancelled")
                return BadRequest(new
                {
                    message = "This registration is already cancelled."
                });

            registration.status = "Cancelled";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Registration cancelled successfully." });
        }
    }

    public class RegistrationInput
    {
        public int EventId { get; set; }
        public int CategoryId { get; set; }
    }
}
