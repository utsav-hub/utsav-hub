namespace Shared.Contracts
{
    public class ScheduleCreatedEvent
    {
        public int ScheduleId { get; set; }
        public string RouteName { get; set; } = string.Empty;
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public string VehicleType { get; set; } = string.Empty;
        public string VehicleNumber { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public int AvailableCapacity { get; set; }
        public decimal PricePerUnit { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class ScheduleUpdatedEvent
    {
        public int ScheduleId { get; set; }
        public string Status { get; set; } = string.Empty;
        public int AvailableCapacity { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ScheduleBookingCreatedEvent
    {
        public int ScheduleId { get; set; }
        public int BookingId { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
    }
}
