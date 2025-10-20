using MassTransit;
using Shared.Contracts;

namespace Schedule.Api.Services
{
    public class ScheduleMessagePublisher
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public ScheduleMessagePublisher(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public async Task PublishScheduleCreatedAsync(Models.Schedule schedule)
        {
            var eventMessage = new ScheduleCreatedEvent
            {
                ScheduleId = schedule.Id,
                RouteName = schedule.RouteName,
                Origin = schedule.Origin,
                Destination = schedule.Destination,
                DepartureTime = schedule.DepartureTime,
                ArrivalTime = schedule.ArrivalTime,
                VehicleType = schedule.VehicleType,
                VehicleNumber = schedule.VehicleNumber,
                Capacity = schedule.Capacity,
                AvailableCapacity = schedule.AvailableCapacity,
                PricePerUnit = schedule.PricePerUnit,
                Status = schedule.Status,
                CreatedAt = schedule.CreatedAt
            };

            await _publishEndpoint.Publish(eventMessage);
        }

        public async Task PublishScheduleUpdatedAsync(Models.Schedule schedule)
        {
            var eventMessage = new ScheduleUpdatedEvent
            {
                ScheduleId = schedule.Id,
                Status = schedule.Status,
                AvailableCapacity = schedule.AvailableCapacity,
                UpdatedAt = schedule.UpdatedAt ?? DateTime.UtcNow
            };

            await _publishEndpoint.Publish(eventMessage);
        }

        public async Task PublishScheduleBookingCreatedAsync(Models.ScheduleBooking booking)
        {
            var eventMessage = new ScheduleBookingCreatedEvent
            {
                ScheduleId = booking.ScheduleId,
                BookingId = booking.BookingId,
                CustomerId = booking.CustomerId,
                Quantity = booking.Quantity,
                TotalPrice = booking.TotalPrice,
                Status = booking.Status,
                BookingDate = booking.BookingDate
            };

            await _publishEndpoint.Publish(eventMessage);
        }
    }
}
