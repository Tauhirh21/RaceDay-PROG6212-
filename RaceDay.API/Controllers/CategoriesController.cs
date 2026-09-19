using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using System.Security.Claims;

namespace RaceDay.API.Controllers
{
    [Route("api")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public CategoriesController(RaceDayDbContext context)
        {
            _context = context;
        }

        // Helper method to get the logged-in user's ID from the JWT token
        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim!);
        }

        // POST: api/events/{eventId}/categories (Organiser only)
        [HttpPost("events/{eventId}/categories")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateCategory(int eventId, CreateCategoryDto dto)
        {
            var userId = GetUserId();
            var eventEntity = await _context.Events.FindAsync(eventId);

            if (eventEntity == null) return NotFound(new { message = "Event not found." });

            // Ensure the organiser owns the event
            if (eventEntity.OrganiserId != userId) return Forbid();

            var category = new Category
            {
                EventId = eventId,
                Name = dto.Name,
                DistanceKm = dto.DistanceKm,
                EntryFee = dto.EntryFee,
                MaxParticipants = dto.MaxParticipants
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategoryById), new { id = category.CategoryId }, new { message = "Category created successfully.", categoryId = category.CategoryId });
        }

        // GET: api/events/{eventId}/categories (Any logged in user)
        [HttpGet("events/{eventId}/categories")]
        [Authorize]
        public async Task<IActionResult> GetCategoriesForEvent(int eventId)
        {
            var categories = await _context.Categories
                .Where(c => c.EventId == eventId)
                .Select(c => new
                {
                    c.CategoryId,
                    c.Name,
                    c.DistanceKm,
                    c.EntryFee,
                    c.MaxParticipants
                })
                .ToListAsync();

            return Ok(categories);
        }

        // GET: api/categories/{id} (Any logged in user)
        [HttpGet("categories/{id}")]
        [Authorize]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound(new { message = "Category not found." });

            return Ok(category);
        }

        // PUT: api/categories/{id} (Organiser only, must own the event)
        [HttpPut("categories/{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateCategory(int id, UpdateCategoryDto dto)
        {
            var userId = GetUserId();
            var category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null) return NotFound(new { message = "Category not found." });

            // Ensure the organiser owns the event this category belongs to
            if (category.Event!.OrganiserId != userId) return Forbid();

            category.Name = dto.Name;
            category.DistanceKm = dto.DistanceKm;
            category.EntryFee = dto.EntryFee;
            category.MaxParticipants = dto.MaxParticipants;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Category updated successfully." });
        }

        // DELETE: api/categories/{id} (Organiser only, must own the event)
        [HttpDelete("categories/{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var userId = GetUserId();
            var category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null) return NotFound(new { message = "Category not found." });

            // Ensure the organiser owns the event this category belongs to
            if (category.Event!.OrganiserId != userId) return Forbid();

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
