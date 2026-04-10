using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DealManagementSystem.Data;
using DealManagementSystem.Entities;
using DealManagementSystem.Validators;
using FluentValidation;

namespace DealManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DealsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IValidator<Hotel> _hotelValidator;

        public DealsController(AppDbContext context, IValidator<Hotel> hotelValidator)
        {
            _context = context;
            _hotelValidator = hotelValidator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var deals = await _context.Deals
                .Include(d => d.Hotels)
                .ToListAsync();

            return Ok(deals);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var deal = await _context.Deals
                .Include(d => d.Hotels)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (deal == null)
                return NotFound();

            return Ok(deal);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Deal deal)
        {
            if (deal.Hotels == null || !deal.Hotels.Any())
                return BadRequest("A deal must have at least one hotel.");

            var slugExists = await _context.Deals
                .AnyAsync(d => d.Slug == deal.Slug);

            if (slugExists)
                return Conflict("Slug must be unique.");

            _context.Deals.Add(deal);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = deal.Id }, deal);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, Deal updatedDeal)
        {
            if (updatedDeal.Hotels == null || !updatedDeal.Hotels.Any())
                return BadRequest("A deal must have at least one hotel.");

            var existingDeal = await _context.Deals
                .Include(d => d.Hotels)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (existingDeal == null)
                return NotFound();

            var duplicateSlug = await _context.Deals
                .AnyAsync(d => d.Slug == updatedDeal.Slug && d.Id != id);

            if (duplicateSlug)
                return Conflict("Slug must be unique.");

            existingDeal.Name = updatedDeal.Name;
            existingDeal.Slug = updatedDeal.Slug;
            existingDeal.Video = updatedDeal.Video;

            var incomingHotelIds = updatedDeal.Hotels
                .Where(h => h.Id > 0)
                .Select(h => h.Id)
                .ToHashSet();

            var hotelsToRemove = existingDeal.Hotels
                .Where(h => !incomingHotelIds.Contains(h.Id))
                .ToList();

            _context.Hotels.RemoveRange(hotelsToRemove);

            foreach (var incomingHotel in updatedDeal.Hotels)
            {
                if (incomingHotel.Id == 0)
                {
                    existingDeal.Hotels.Add(new Hotel
                    {
                        Name = incomingHotel.Name,
                        Rate = incomingHotel.Rate,
                        Amenities = incomingHotel.Amenities,
                        DealId = id
                    });
                    continue;
                }

                var existingHotel = existingDeal.Hotels
                    .FirstOrDefault(h => h.Id == incomingHotel.Id);

                if (existingHotel == null)
                    return BadRequest($"Hotel id {incomingHotel.Id} does not belong to deal {id}.");

                existingHotel.Name = incomingHotel.Name;
                existingHotel.Rate = incomingHotel.Rate;
                existingHotel.Amenities = incomingHotel.Amenities;
            }

            await _context.SaveChangesAsync();

            return Ok(existingDeal);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteDeal(int id)
        {
            var deal = await _context.Deals.FindAsync(id);
            if (deal == null)
                return NotFound();

            _context.Deals.Remove(deal);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("{dealId:int}/hotels")]
        public async Task<IActionResult> GetHotels(int dealId)
        {
            var dealExists = await _context.Deals.AnyAsync(d => d.Id == dealId);
            if (!dealExists)
                return NotFound();

            var hotels = await _context.Hotels
                .Where(h => h.DealId == dealId)
                .ToListAsync();

            return Ok(hotels);
        }

        [HttpGet("{dealId:int}/hotels/{hotelId:int}")]
        public async Task<IActionResult> GetHotelById(int dealId, int hotelId)
        {
            var hotel = await _context.Hotels
                .FirstOrDefaultAsync(h => h.DealId == dealId && h.Id == hotelId);

            if (hotel == null)
                return NotFound();

            return Ok(hotel);
        }

        [HttpPost("{dealId:int}/hotels")]
        public async Task<IActionResult> CreateHotel(int dealId, Hotel hotel)
        {
            var deal = await _context.Deals.FindAsync(dealId);
            if (deal == null)
                return NotFound();

            var validationResult = await _hotelValidator.ValidateAsync(hotel);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

            hotel.DealId = dealId;

            _context.Hotels.Add(hotel);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetHotelById), new { dealId, hotelId = hotel.Id }, hotel);
        }

        [HttpPut("{dealId:int}/hotels/{hotelId:int}")]
        public async Task<IActionResult> UpdateHotel(int dealId, int hotelId, Hotel updatedHotel)
        {
            var hotel = await _context.Hotels
                .FirstOrDefaultAsync(h => h.DealId == dealId && h.Id == hotelId);

            if (hotel == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(updatedHotel.Name))
                return BadRequest("Hotel name is required.");

            if (updatedHotel.Rate < 1.0m || updatedHotel.Rate > 5.0m)
                return BadRequest("Rate must be between 1.0 and 5.0.");

            hotel.Name = updatedHotel.Name;
            hotel.Rate = updatedHotel.Rate;
            hotel.Amenities = updatedHotel.Amenities;

            await _context.SaveChangesAsync();

            return Ok(hotel);
        }

        [HttpDelete("{dealId:int}/hotels/{hotelId:int}")]
        public async Task<IActionResult> DeleteHotel(int dealId, int hotelId)
        {
            var hotel = await _context.Hotels
                .FirstOrDefaultAsync(h => h.DealId == dealId && h.Id == hotelId);

            if (hotel == null)
                return NotFound();

            var hotelCount = await _context.Hotels
                .CountAsync(h => h.DealId == dealId);

            if (hotelCount <= 1)
                return BadRequest("A deal must have at least one hotel.");

            _context.Hotels.Remove(hotel);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}