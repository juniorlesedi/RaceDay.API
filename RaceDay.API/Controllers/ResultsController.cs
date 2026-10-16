
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/Results/my
        [HttpGet("my")]
        public async Task<IActionResult> GetMyResults()
        {
            if (HttpContext.Session.GetString("Role") != "Participant")
                return Unauthorized(new
                {
                    message = "Log in as a Participant first."
                });

            int? participantId = HttpContext.Session.GetInt32("UserId");

            if (participantId == null)
                return Unauthorized();

            var results = await _context.Result
                .AsNoTracking()
                .Where(r => r.participantID == participantId.Value)
                .OrderBy(r => r.position)
                .ToListAsync();

            return Ok(results);
        }

        // GET: api/Results/event/1
        [HttpGet("event/{eventId:int}")]
        public async Task<IActionResult> GetEventResults(int eventId)
        {
            var raceEvent = await _context.Event
                .FirstOrDefaultAsync(e => e.eventID == eventId);

            if (raceEvent == null)
                return NotFound(new { message = "Event not found." });

            // The organiser who owns the event can view its results.
            if (HttpContext.Session.GetString("Role") == "Organiser")
            {
                int? organiserId = HttpContext.Session.GetInt32("UserId");

                if (organiserId == null)
                    return Unauthorized();

                if (raceEvent.organiserID != organiserId.Value)
                    return Forbid();
            }
            else if (HttpContext.Session.GetString("Role") != "Participant")
            {
                return Unauthorized(new { message = "Please log in first." });
            }

            var results = await _context.Result
                .AsNoTracking()
                .Where(r => r.eventID == eventId)
                .OrderBy(r => r.position)
                .ToListAsync();

            return Ok(results);
        }

        // POST: api/Results
        [HttpPost]
        public async Task<IActionResult> CreateResult(ResultInput input)
        {
            if (HttpContext.Session.GetString("Role") != "Organiser")
                return Unauthorized(new
                {
                    message = "Log in as an Organiser first."
                });

            int? organiserId = HttpContext.Session.GetInt32("UserId");

            if (organiserId == null)
                return Unauthorized();

            if (input.FinishTime <= TimeSpan.Zero ||
                input.Position <= 0)
            {
                return BadRequest(new
                {
                    message = "Finish time and position must be positive."
                });
            }

            var registration = await _context.Registration
                .FirstOrDefaultAsync(r =>
                    r.registrationID == input.RegistrationId);

            if (registration == null)
                return NotFound(new { message = "Registration not found." });

            if (registration.status == "Cancelled")
                return BadRequest(new
                {
                    message = "Cannot record a result for a cancelled registration."
                });

            var raceEvent = await _context.Event
                .FirstOrDefaultAsync(e => e.eventID == registration.eventID);

            if (raceEvent == null ||
                raceEvent.organiserID != organiserId.Value)
                return Forbid();

            bool resultExists = await _context.Result.AnyAsync(r =>
                r.registrationID == registration.registrationID);

            if (resultExists)
                return Conflict(new
                {
                    message = "A result already exists for this registration."
                });

            int newId = (await _context.Result
                .MaxAsync(r => (int?)r.resultID) ?? 0) + 1;

            var result = new Result
            {
                resultID = newId,
                registrationID = registration.registrationID,
                participantID = registration.participantID,
                eventID = registration.eventID,
                categoryID = registration.categoryID,
                finish_time = input.FinishTime,
                position = input.Position,
                split_times = input.SplitTimes,
                created_at = DateTime.Now
            };

            _context.Result.Add(result);
            await _context.SaveChangesAsync();

            return Ok(result);
        }
    }

    public class ResultInput
    {
        public int RegistrationId { get; set; }
        public TimeSpan FinishTime { get; set; }
        public int Position { get; set; }
        public string? SplitTimes { get; set; }
    }
}

