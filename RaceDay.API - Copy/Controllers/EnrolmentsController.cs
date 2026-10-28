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
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EnrolmentsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // Helper method to get the logged-in user's ID
        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim!);
        }

        // POST: api/enrolments (Participant only)
        [HttpPost]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> CreateEnrolment(CreateEnrolmentDto dto)
        {
            var userId = GetUserId();
            var category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryId == dto.CategoryId);

            if (category == null) return NotFound(new { message = "Category not found." });

            // Check if participant is already enrolled in this category
            var existing = await _context.Enrolments
                .AnyAsync(e => e.UserId == userId && e.CategoryId == dto.CategoryId);

            if (existing) return Conflict(new { message = "You are already enrolled in this category." });

            // Check if category is full
            if (category.MaxParticipants > 0)
            {
                var currentCount = await _context.Enrolments.CountAsync(e => e.CategoryId == dto.CategoryId);
                if (currentCount >= category.MaxParticipants)
                {
                    return BadRequest(new { message = "This category is full." });
                }
            }

            var enrolment = new Enrolment
            {
                UserId = userId,
                CategoryId = dto.CategoryId,
                EnrolmentDate = DateTime.UtcNow,
                Status = "Confirmed"
            };

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEnrolmentById), new { id = enrolment.EnrolmentId }, new { message = "Enrolment successful.", enrolmentId = enrolment.EnrolmentId });
        }

        // GET: api/enrolments/mine (Participant only)
        [HttpGet("mine")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> GetMyEnrolments()
        {
            var userId = GetUserId();
            var enrolments = await _context.Enrolments
                .Where(e => e.UserId == userId)
                .Include(e => e.Category)
                    .ThenInclude(c => c!.Event)
                .Select(e => new
                {
                    e.EnrolmentId,
                    e.EnrolmentDate,
                    e.BibNumber,
                    e.Status,
                    CategoryName = e.Category!.Name,
                    DistanceKm = e.Category.DistanceKm,
                    EventName = e.Category.Event!.Name,
                    EventDate = e.Category.Event.EventDate,
                    EventLocation = e.Category.Event.Location
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        // GET: api/enrolments/{id} (Participant owns it OR Organiser owns the event)
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetEnrolmentById(int id)
        {
            var userId = GetUserId();
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var enrolment = await _context.Enrolments
                .Include(e => e.Category)
                    .ThenInclude(c => c!.Event)
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.EnrolmentId == id);

            if (enrolment == null) return NotFound(new { message = "Enrolment not found." });

            // Participant can only view their own enrolment
            if (userRole == "Participant" && enrolment.UserId != userId) return Forbid();

            // Organiser can only view enrolments for their own events
            if (userRole == "Organiser" && enrolment.Category!.Event!.OrganiserId != userId) return Forbid();

            return Ok(enrolment);
        }

        // GET: api/events/{eventId}/enrolments (Organiser only, owns the event)
        [HttpGet("/api/events/{eventId}/enrolments")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> GetEnrolmentsForEvent(int eventId)
        {
            var userId = GetUserId();
            var eventEntity = await _context.Events.FindAsync(eventId);

            if (eventEntity == null) return NotFound(new { message = "Event not found." });
            if (eventEntity.OrganiserId != userId) return Forbid();

            var enrolments = await _context.Enrolments
                .Include(e => e.User)
                .Include(e => e.Category)
                .Where(e => e.Category!.EventId == eventId)
                .Select(e => new
                {
                    e.EnrolmentId,
                    ParticipantName = e.User!.FullName,
                    ParticipantEmail = e.User.Email,
                    CategoryName = e.Category!.Name,
                    e.EnrolmentDate,
                    e.BibNumber,
                    e.Status
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        // DELETE: api/enrolments/{id} (Participant only, must own it)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> CancelEnrolment(int id)
        {
            var userId = GetUserId();
            var enrolment = await _context.Enrolments.FindAsync(id);

            if (enrolment == null) return NotFound(new { message = "Enrolment not found." });
            if (enrolment.UserId != userId) return Forbid();

            _context.Enrolments.Remove(enrolment);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}