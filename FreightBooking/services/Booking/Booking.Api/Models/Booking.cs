using System.ComponentModel.DataAnnotations;

namespace Booking.Api.Models
{
    public class Booking
    {
        public int Id { get; set; }
        
        [Required]
        public string CustomerId { get; set; } = string.Empty;
        
        [Required]
        public string CustomerName { get; set; } = string.Empty;
        
        [Required]
        public string CustomerEmail { get; set; } = string.Empty;
        
        [Required]
        public string Origin { get; set; } = string.Empty;
        
        [Required]
        public string Destination { get; set; } = string.Empty;
        
        public DateTime BookingDate { get; set; }
        
        public DateTime DepartureDate { get; set; }
        
        public DateTime? ArrivalDate { get; set; }
        
        [Required]
        public string CargoType { get; set; } = string.Empty;
        
        public decimal Weight { get; set; }
        
        public decimal Volume { get; set; }
        
        public decimal Price { get; set; }
        
        public string Status { get; set; } = "Pending";
        
        public string? Notes { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public virtual ICollection<BookingItem> BookingItems { get; set; } = new List<BookingItem>();
    }
}
