using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Booking.Api.Data;
using Booking.Api.Models;
using Booking.Api.Services;

namespace Booking.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BookingsController : ControllerBase
    {
        private readonly BookingDbContext _context;
        private readonly BookingMessagePublisher _messagePublisher;

        public BookingsController(BookingDbContext context, BookingMessagePublisher messagePublisher)
        {
            _context = context;
            _messagePublisher = messagePublisher;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Booking>>> GetBookings()
        {
            return await _context.Bookings
                .Include(b => b.BookingItems)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Booking>> GetBooking(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.BookingItems)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            return booking;
        }

        [HttpPost]
        public async Task<ActionResult<Booking>> CreateBooking([FromBody] CreateBookingRequest request)
        {
            var booking = new Booking
            {
                CustomerId = request.CustomerId,
                CustomerName = request.CustomerName,
                CustomerEmail = request.CustomerEmail,
                Origin = request.Origin,
                Destination = request.Destination,
                BookingDate = DateTime.UtcNow,
                DepartureDate = request.DepartureDate,
                ArrivalDate = request.ArrivalDate,
                CargoType = request.CargoType,
                Weight = request.Weight,
                Volume = request.Volume,
                Price = request.Price,
                Status = "Pending",
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            // Publish booking created event
            await _messagePublisher.PublishBookingCreatedAsync(booking);

            return CreatedAtAction(nameof(GetBooking), new { id = booking.Id }, booking);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBooking(int id, [FromBody] UpdateBookingRequest request)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            booking.CustomerName = request.CustomerName;
            booking.CustomerEmail = request.CustomerEmail;
            booking.Origin = request.Origin;
            booking.Destination = request.Destination;
            booking.DepartureDate = request.DepartureDate;
            booking.ArrivalDate = request.ArrivalDate;
            booking.CargoType = request.CargoType;
            booking.Weight = request.Weight;
            booking.Volume = request.Volume;
            booking.Price = request.Price;
            booking.Status = request.Status;
            booking.Notes = request.Notes;
            booking.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Publish booking updated event
            await _messagePublisher.PublishBookingUpdatedAsync(booking);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            // Publish booking cancelled event
            await _messagePublisher.PublishBookingCancelledAsync(id, "Booking deleted");

            return NoContent();
        }
    }

    public class CreateBookingRequest
    {
        public string CustomerId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public DateTime DepartureDate { get; set; }
        public DateTime? ArrivalDate { get; set; }
        public string CargoType { get; set; } = string.Empty;
        public decimal Weight { get; set; }
        public decimal Volume { get; set; }
        public decimal Price { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateBookingRequest
    {
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public DateTime DepartureDate { get; set; }
        public DateTime? ArrivalDate { get; set; }
        public string CargoType { get; set; } = string.Empty;
        public decimal Weight { get; set; }
        public decimal Volume { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}
