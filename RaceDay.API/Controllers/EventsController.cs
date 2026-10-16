
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // Both roles and unauthenticated visitors can view events.
        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var events = await _context.Event
                .AsNoTracking()
                .OrderBy(e => e.event_date)
                .ToListAsync();

            return Ok(events);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetEvent(int id)
        {
            var raceEvent = await _context.Event.FindAsync(id);

            if (raceEvent == null)
                return NotFound(new { message = "Event not found." });

            return Ok(raceEvent);
        }

        // Only Organisers can create events.
        [HttpPost]
        public async Task<IActionResult> CreateEvent(EventInput input)
        {
            var organiserId = GetOrganiserId();

            if (organiserId == null)
                return Unauthorized(new { message = "Log in as an Organiser first." });

            if (!IsValidEvent(input))
                return BadRequest(new
                {
                    message = "Provide all required fields. Event type must be Run, Walk or Cycle, and distance must be positive."
                });

            var raceEvent = new RaceEvent
            {
                eventID = (await _context.Event
                    .MaxAsync(e => (int?)e.eventID) ?? 0) + 1,
                organiserID = organiserId.Value,
                name = input.Name.Trim(),
                description = input.Description.Trim(),
                event_date = input.EventDate,
                location = input.Location.Trim(),
                Distance_km = input.DistanceKm,
                event_type = input.EventType.Trim(),
                status = "Upcoming",
                created_at = DateTime.Now
            };

            _context.Event.Add(raceEvent);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEvent),
                new { id = raceEvent.eventID },
                raceEvent);
        }

        // An Organiser can update only their own event.
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateEvent(int id, EventInput input)
        {
            var organiserId = GetOrganiserId();

            if (organiserId == null)
                return Unauthorized(new { message = "Organiser login required." });

            var raceEvent = await _context.Event.FindAsync(id);

            if (raceEvent == null)
                return NotFound(new { message = "Event not found." });

            if (raceEvent.organiserID != organiserId.Value)
                return Forbid();

            if (!IsValidEvent(input))
                return BadRequest(new { message = "Check the event details and distance." });

            raceEvent.name = input.Name.Trim();
            raceEvent.description = input.Description.Trim();
            raceEvent.event_date = input.EventDate;
            raceEvent.location = input.Location.Trim();
            raceEvent.Distance_km = input.DistanceKm;
            raceEvent.event_type = input.EventType.Trim();

            await _context.SaveChangesAsync();

            return Ok(raceEvent);
        }

        // An Organiser can delete only their own event.
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var organiserId = GetOrganiserId();

            if (organiserId == null)
                return Unauthorized(new { message = "Organiser login required." });

            var raceEvent = await _context.Event.FindAsync(id);

            if (raceEvent == null)
                return NotFound(new { message = "Event not found." });

            if (raceEvent.organiserID != organiserId.Value)
                return Forbid();

            bool hasCategories = await _context.Category
                .AnyAsync(c => c.eventID == id);

            bool hasRegistrations = await _context.Registration
                .AnyAsync(r => r.eventID == id);

            if (hasCategories || hasRegistrations)
                return Conflict(new
                {
                    message = "This event has categories or registrations and cannot be deleted."
                });

            _context.Event.Remove(raceEvent);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Event deleted successfully." });
        }

        private int? GetOrganiserId()
        {
            if (HttpContext.Session.GetString("Role") != "Organiser")
                return null;

            return HttpContext.Session.GetInt32("UserId");
        }

        private static bool IsValidEvent(EventInput input)
        {
            return !string.IsNullOrWhiteSpace(input.Name)
                && input.Name.Length <= 50
                && !string.IsNullOrWhiteSpace(input.Description)
                && input.Description.Length <= 100
                && !string.IsNullOrWhiteSpace(input.Location)
                && input.Location.Length <= 50
                && input.DistanceKm > 0
                && new[] { "Run", "Walk", "Cycle" }.Contains(
                    input.EventType?.Trim(),
                    StringComparer.OrdinalIgnoreCase);
        }
    }

    public class EventInput
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = "";
        public decimal DistanceKm { get; set; }
        public string EventType { get; set; } = "";
    }
}
