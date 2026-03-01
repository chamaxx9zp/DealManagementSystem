using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DealManagementSystem.Data;
using DealManagementSystem.Entities;

namespace DealManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DealsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DealsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var deals = await _context.Deals
                .Include(d => d.Hotels)
                .ToListAsync();

            return Ok(deals);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Deal deal)
        {
            if (deal.Hotels == null || !deal.Hotels.Any())
                return BadRequest("A deal must have at least one hotel.");

            _context.Deals.Add(deal);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAll), new { id = deal.Id }, deal);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHotel(int id)
        {
            var hotel = await _context.Hotels.FindAsync(id);
            if (hotel == null)
                return NotFound();

            var hotelCount = await _context.Hotels
                .CountAsync(h => h.DealId == hotel.DealId);

            if (hotelCount <= 1)
                return BadRequest("A deal must have at least one hotel.");

            _context.Hotels.Remove(hotel);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}