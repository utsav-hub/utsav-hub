using System.ComponentModel.DataAnnotations;

namespace Booking.Api.Models
{
    public class BookingItem
    {
        public int Id { get; set; }
        
        public int BookingId { get; set; }
        
        [Required]
        public string ItemName { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        public int Quantity { get; set; }
        
        public decimal Weight { get; set; }
        
        public decimal Volume { get; set; }
        
        public decimal UnitPrice { get; set; }
        
        public decimal TotalPrice { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation property
        public virtual Booking Booking { get; set; } = null!;
    }
}
