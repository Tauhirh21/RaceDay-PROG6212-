using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using System.Security.Claims;

namespace RaceDay.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // Helper method to get the logged-in user's ID from the JWT token
        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim!);
        }

        // GET: api/events (Both roles must be able to view events)
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllEvents()
        {
            var events = await _context.Events
                .Include(e => e.Organiser)
                .Select(e => new
                {
                    e.EventId,
                    e.Name,
                    e.Description,
                    e.EventDate,
                    e.Location,
                    e.Province,
                    e.EventType,
                    OrganiserName = e.Organiser!.FullName,
                    e.CreatedAt
                })
                .ToListAsync();

            return Ok(events);
        }

        // GET: api/events/{id}
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetEventById(int id)
        {
            var ev = await _context.Events
                .Include(e => e.Organiser)
                .Where(e => e.EventId == id)
                .Select(e => new
                {
                    e.EventId,
                    e.Name,
                    e.Description,
                    e.EventDate,
                    e.Location,
                    e.Province,
                    e.EventType,
                    OrganiserName = e.Organiser!.FullName,
                    e.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (ev == null) return NotFound(new { message = "Event not found." });

            return Ok(ev);
        }

        // POST: api/events (Organiser only)
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateEvent(CreateEventDto dto)
        {
            var userId = GetUserId();

            var newEvent = new Event
            {
                Name = dto.Name,
                Description = dto.Description,
                EventDate = dto.EventDate,
                Location = dto.Location,
                Province = dto.Province,
                EventType = dto.EventType,
                OrganiserId = userId
            };

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEventById), new { id = newEvent.EventId }, new { message = "Event created successfully.", eventId = newEvent.EventId });
        }

        // PUT: api/events/{id} (Organiser only, must own the event)
        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateEvent(int id, UpdateEventDto dto)
        {
            var userId = GetUserId();
            var existingEvent = await _context.Events.FindAsync(id);

            if (existingEvent == null) return NotFound(new { message = "Event not found." });

            // Ensure the organiser owns this event
            if (existingEvent.OrganiserId != userId)
            {
                return Forbid();
            }

            existingEvent.Name = dto.Name;
            existingEvent.Description = dto.Description;
            existingEvent.EventDate = dto.EventDate;
            existingEvent.Location = dto.Location;
            existingEvent.Province = dto.Province;
            existingEvent.EventType = dto.EventType;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Event updated successfully." });
        }

        // DELETE: api/events/{id} (Organiser only, must own the event)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var userId = GetUserId();
            var existingEvent = await _context.Events.FindAsync(id);

            if (existingEvent == null) return NotFound(new { message = "Event not found." });

            // Ensure the organiser owns this event
            if (existingEvent.OrganiserId != userId)
            {
                return Forbid();
            }

            _context.Events.Remove(existingEvent);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
