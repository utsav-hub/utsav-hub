using MassTransit;
using Shared.Contracts;

namespace Schedule.Api.Services
{
    public class BookingEventConsumer : IConsumer<BookingCreatedEvent>
    {
        private readonly ILogger<BookingEventConsumer> _logger;

        public BookingEventConsumer(ILogger<BookingEventConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<BookingCreatedEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation("Received BookingCreatedEvent: BookingId={BookingId}, Customer={CustomerName}", 
                message.BookingId, message.CustomerName);

            // Here you can add logic to handle booking creation
            // For example, automatically create schedule bookings, update availability, etc.
            
            await Task.CompletedTask;
        }
    }
}
