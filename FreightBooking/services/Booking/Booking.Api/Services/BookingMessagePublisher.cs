using MassTransit;
using Shared.Contracts;

namespace Booking.Api.Services
{
    public class BookingMessagePublisher
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public BookingMessagePublisher(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public async Task PublishBookingCreatedAsync(Models.Booking booking)
        {
            var eventMessage = new BookingCreatedEvent
            {
                BookingId = booking.Id,
                CustomerId = booking.CustomerId,
                CustomerName = booking.CustomerName,
                CustomerEmail = booking.CustomerEmail,
                Origin = booking.Origin,
                Destination = booking.Destination,
                DepartureDate = booking.DepartureDate,
                ArrivalDate = booking.ArrivalDate,
                CargoType = booking.CargoType,
                Weight = booking.Weight,
                Volume = booking.Volume,
                Price = booking.Price,
                Status = booking.Status,
                CreatedAt = booking.CreatedAt
            };

            await _publishEndpoint.Publish(eventMessage);
        }

        public async Task PublishBookingUpdatedAsync(Models.Booking booking)
        {
            var eventMessage = new BookingUpdatedEvent
            {
                BookingId = booking.Id,
                Status = booking.Status,
                UpdatedAt = booking.UpdatedAt ?? DateTime.UtcNow
            };

            await _publishEndpoint.Publish(eventMessage);
        }

        public async Task PublishBookingCancelledAsync(int bookingId, string reason)
        {
            var eventMessage = new BookingCancelledEvent
            {
                BookingId = bookingId,
                Reason = reason,
                CancelledAt = DateTime.UtcNow
            };

            await _publishEndpoint.Publish(eventMessage);
        }
    }
}
