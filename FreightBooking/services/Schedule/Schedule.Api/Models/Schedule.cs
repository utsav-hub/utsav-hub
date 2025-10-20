using System.ComponentModel.DataAnnotations;

namespace Schedule.Api.Models
{
    public class Schedule
    {
        public int Id { get; set; }
        
        [Required]
        public string RouteName { get; set; } = string.Empty;
        
        [Required]
        public string Origin { get; set; } = string.Empty;
        
        [Required]
        public string Destination { get; set; } = string.Empty;
        
        public DateTime DepartureTime { get; set; }
        
        public DateTime ArrivalTime { get; set; }
        
        [Required]
        public string VehicleType { get; set; } = string.Empty;
        
        [Required]
        public string VehicleNumber { get; set; } = string.Empty;
        
        public int Capacity { get; set; }
        
        public int AvailableCapacity { get; set; }
        
        public decimal PricePerUnit { get; set; }
        
        public string Status { get; set; } = "Scheduled";
        
        public string? DriverName { get; set; }
        
        public string? DriverContact { get; set; }
        
        public string? Notes { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public virtual ICollection<ScheduleBooking> ScheduleBookings { get; set; } = new List<ScheduleBooking>();
    }
}
