
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public CategoriesController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/Categories
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Category
                .AsNoTracking()
                .OrderBy(c => c.categoryID)
                .ToListAsync();

            return Ok(categories);
        }

        // GET: api/Categories/1
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCategory(int id)
        {
            var category = await _context.Category.FindAsync(id);

            if (category == null)
                return NotFound(new { message = "Category not found." });

            return Ok(category);
        }

        // GET: api/Categories/event/1
        [HttpGet("event/{eventId:int}")]
        public async Task<IActionResult> GetEventCategories(int eventId)
        {
            bool eventExists = await _context.Event
                .AnyAsync(e => e.eventID == eventId);

            if (!eventExists)
                return NotFound(new { message = "Event not found." });

            var categories = await _context.Category
                .AsNoTracking()
                .Where(c => c.eventID == eventId)
                .ToListAsync();

            return Ok(categories);
        }

        // POST: api/Categories
        [HttpPost]
        public async Task<IActionResult> CreateCategory(CategoryInput input)
        {
            int? organiserId = GetOrganiserId();

            if (organiserId == null)
                return Unauthorized(new
                {
                    message = "Log in as an Organiser first."
                });

            if (string.IsNullOrWhiteSpace(input.Name) ||
                string.IsNullOrWhiteSpace(input.AgeGroup) ||
                string.IsNullOrWhiteSpace(input.Gender) ||
                input.DistanceKm <= 0 ||
                input.MaxParticipants <= 0)
            {
                return BadRequest(new
                {
                    message = "Enter a name, age group, gender, positive distance and maximum participants."
                });
            }

            var raceEvent = await _context.Event
                .FirstOrDefaultAsync(e => e.eventID == input.EventId);

            if (raceEvent == null)
                return NotFound(new { message = "Event not found." });

            if (raceEvent.organiserID != organiserId.Value)
                return Forbid();

            if (input.Name.Length > 50 ||
                input.AgeGroup.Length > 50 ||
                input.Gender.Length > 10)
            {
                return BadRequest(new
                {
                    message = "Some category fields exceed the allowed length."
                });
            }

            int newId = (await _context.Category
                .MaxAsync(c => (int?)c.categoryID) ?? 0) + 1;

            var category = new Category
            {
                categoryID = newId,
                eventID = input.EventId,
                name = input.Name.Trim(),
                agegroup = input.AgeGroup.Trim(),
                gender = input.Gender.Trim(),
                distancekm = input.DistanceKm,
                max_participants = input.MaxParticipants
            };

            _context.Category.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetCategory),
                new { id = category.categoryID },
                category);
        }

        // PUT: api/Categories/1
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateCategory(
            int id, CategoryInput input)
        {
            int? organiserId = GetOrganiserId();

            if (organiserId == null)
                return Unauthorized(new
                {
                    message = "Organiser login required."
                });

            var category = await _context.Category
                .FirstOrDefaultAsync(c => c.categoryID == id);

            if (category == null)
                return NotFound(new { message = "Category not found." });

            var raceEvent = await _context.Event
                .FirstOrDefaultAsync(e => e.eventID == category.eventID);

            if (raceEvent == null ||
                raceEvent.organiserID != organiserId.Value)
                return Forbid();

            if (input.EventId != category.eventID ||
                string.IsNullOrWhiteSpace(input.Name) ||
                string.IsNullOrWhiteSpace(input.AgeGroup) ||
                string.IsNullOrWhiteSpace(input.Gender) ||
                input.DistanceKm <= 0 ||
                input.MaxParticipants <= 0)
            {
                return BadRequest(new
                {
                    message = "Check the category details. Its event cannot be changed."
                });
            }

            if (input.Name.Length > 50 ||
                input.AgeGroup.Length > 50 ||
                input.Gender.Length > 10)
            {
                return BadRequest(new
                {
                    message = "Some category fields exceed the allowed length."
                });
            }

            category.name = input.Name.Trim();
            category.agegroup = input.AgeGroup.Trim();
            category.gender = input.Gender.Trim();
            category.distancekm = input.DistanceKm;
            category.max_participants = input.MaxParticipants;

            await _context.SaveChangesAsync();

            return Ok(category);
        }

        // DELETE: api/Categories/1
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            int? organiserId = GetOrganiserId();

            if (organiserId == null)
                return Unauthorized(new
                {
                    message = "Organiser login required."
                });

            var category = await _context.Category
                .FirstOrDefaultAsync(c => c.categoryID == id);

            if (category == null)
                return NotFound(new { message = "Category not found." });

            var raceEvent = await _context.Event
                .FirstOrDefaultAsync(e => e.eventID == category.eventID);

            if (raceEvent == null ||
                raceEvent.organiserID != organiserId.Value)
                return Forbid();

            bool hasRegistrations = await _context.Registration
                .AnyAsync(r => r.categoryID == id);

            if (hasRegistrations)
                return Conflict(new
                {
                    message = "This category has registrations and cannot be deleted."
                });

            _context.Category.Remove(category);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Category deleted successfully."
            });
        }

        private int? GetOrganiserId()
        {
            if (HttpContext.Session.GetString("Role") != "Organiser")
                return null;

            return HttpContext.Session.GetInt32("UserId");
        }
    }

    public class CategoryInput
    {
        public int EventId { get; set; }
        public string Name { get; set; } = "";
        public string AgeGroup { get; set; } = "";
        public string Gender { get; set; } = "";
        public decimal DistanceKm { get; set; }
        public int MaxParticipants { get; set; }
    }
}
