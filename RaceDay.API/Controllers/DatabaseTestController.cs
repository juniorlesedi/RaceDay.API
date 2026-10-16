
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DatabaseTestController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public DatabaseTestController(RaceDayDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                bool connected = await _context.Database.CanConnectAsync();

                if (!connected)
                {
                    return StatusCode(500, new
                    {
                        message = "Could not connect to the RaceDay database."
                    });
                }

                int eventCount = await _context.Event.CountAsync();

                return Ok(new
                {
                    message = "Successfully connected to RaceDay!",
                    databaseConnected = true,
                    eventsFound = eventCount
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Database test failed.",
                    error = ex.Message
                });
            }
        }
    }
}

