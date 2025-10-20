using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Schedule.Api.Data;
using Schedule.Api.Models;
using Schedule.Api.Services;

namespace Schedule.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SchedulesController : ControllerBase
    {
        private readonly ScheduleDbContext _context;
        private readonly ScheduleMessagePublisher _messagePublisher;

        public SchedulesController(ScheduleDbContext context, ScheduleMessagePublisher messagePublisher)
        {
            _context = context;
            _messagePublisher = messagePublisher;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Schedule>>> GetSchedules()
        {
            return await _context.Schedules
                .Include(s => s.ScheduleBookings)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Schedule>> GetSchedule(int id)
        {
            var schedule = await _context.Schedules
                .Include(s => s.ScheduleBookings)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (schedule == null)
            {
                return NotFound();
            }

            return schedule;
        }

        [HttpPost]
        public async Task<ActionResult<Schedule>> CreateSchedule([FromBody] CreateScheduleRequest request)
        {
            var schedule = new Schedule
            {
                RouteName = request.RouteName,
                Origin = request.Origin,
                Destination = request.Destination,
                DepartureTime = request.DepartureTime,
                ArrivalTime = request.ArrivalTime,
                VehicleType = request.VehicleType,
                VehicleNumber = request.VehicleNumber,
                Capacity = request.Capacity,
                AvailableCapacity = request.Capacity,
                PricePerUnit = request.PricePerUnit,
                Status = "Scheduled",
                DriverName = request.DriverName,
                DriverContact = request.DriverContact,
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow
            };

            _context.Schedules.Add(schedule);
            await _context.SaveChangesAsync();

            // Publish schedule created event
            await _messagePublisher.PublishScheduleCreatedAsync(schedule);

            return CreatedAtAction(nameof(GetSchedule), new { id = schedule.Id }, schedule);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSchedule(int id, [FromBody] UpdateScheduleRequest request)
        {
            var schedule = await _context.Schedules.FindAsync(id);
            if (schedule == null)
            {
                return NotFound();
            }

            schedule.RouteName = request.RouteName;
            schedule.Origin = request.Origin;
            schedule.Destination = request.Destination;
            schedule.DepartureTime = request.DepartureTime;
            schedule.ArrivalTime = request.ArrivalTime;
            schedule.VehicleType = request.VehicleType;
            schedule.VehicleNumber = request.VehicleNumber;
            schedule.Capacity = request.Capacity;
            schedule.PricePerUnit = request.PricePerUnit;
            schedule.Status = request.Status;
            schedule.DriverName = request.DriverName;
            schedule.DriverContact = request.DriverContact;
            schedule.Notes = request.Notes;
            schedule.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Publish schedule updated event
            await _messagePublisher.PublishScheduleUpdatedAsync(schedule);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            var schedule = await _context.Schedules.FindAsync(id);
            if (schedule == null)
            {
                return NotFound();
            }

            _context.Schedules.Remove(schedule);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("{id}/book")]
        public async Task<ActionResult<ScheduleBooking>> BookSchedule(int id, [FromBody] BookScheduleRequest request)
        {
            var schedule = await _context.Schedules.FindAsync(id);
            if (schedule == null)
            {
                return NotFound("Schedule not found");
            }

            if (schedule.AvailableCapacity < request.Quantity)
            {
                return BadRequest("Not enough capacity available");
            }

            var booking = new ScheduleBooking
            {
                ScheduleId = id,
                BookingId = request.BookingId,
                CustomerId = request.CustomerId,
                Quantity = request.Quantity,
                TotalPrice = request.Quantity * schedule.PricePerUnit,
                Status = "Confirmed",
                BookingDate = DateTime.UtcNow,
                Notes = request.Notes
            };

            schedule.AvailableCapacity -= request.Quantity;
            schedule.UpdatedAt = DateTime.UtcNow;

            _context.ScheduleBookings.Add(booking);
            await _context.SaveChangesAsync();

            // Publish schedule booking created event
            await _messagePublisher.PublishScheduleBookingCreatedAsync(booking);

            return CreatedAtAction(nameof(GetSchedule), new { id = booking.Id }, booking);
        }
    }

    public class CreateScheduleRequest
    {
        public string RouteName { get; set; } = string.Empty;
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public string VehicleType { get; set; } = string.Empty;
        public string VehicleNumber { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public decimal PricePerUnit { get; set; }
        public string? DriverName { get; set; }
        public string? DriverContact { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateScheduleRequest
    {
        public string RouteName { get; set; } = string.Empty;
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public string VehicleType { get; set; } = string.Empty;
        public string VehicleNumber { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public decimal PricePerUnit { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? DriverName { get; set; }
        public string? DriverContact { get; set; }
        public string? Notes { get; set; }
    }

    public class BookScheduleRequest
    {
        public int BookingId { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string? Notes { get; set; }
    }
}
