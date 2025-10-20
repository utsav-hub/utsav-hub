using System.ComponentModel.DataAnnotations;

namespace Schedule.Api.Models
{
    public class ScheduleBooking
    {
        public int Id { get; set; }
        
        public int ScheduleId { get; set; }
        
        public int BookingId { get; set; }
        
        [Required]
        public string CustomerId { get; set; } = string.Empty;
        
        public int Quantity { get; set; }
        
        public decimal TotalPrice { get; set; }
        
        public string Status { get; set; } = "Confirmed";
        
        public DateTime BookingDate { get; set; } = DateTime.UtcNow;
        
        public string? Notes { get; set; }
        
        // Navigation properties
        public virtual Schedule Schedule { get; set; } = null!;
    }
}
