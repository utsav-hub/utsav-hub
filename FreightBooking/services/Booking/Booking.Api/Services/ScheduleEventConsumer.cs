using MassTransit;
using Shared.Contracts;

namespace Booking.Api.Services
{
    public class ScheduleEventConsumer : IConsumer<ScheduleCreatedEvent>
    {
        private readonly ILogger<ScheduleEventConsumer> _logger;

        public ScheduleEventConsumer(ILogger<ScheduleEventConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ScheduleCreatedEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation("Received ScheduleCreatedEvent: ScheduleId={ScheduleId}, Route={RouteName}", 
                message.ScheduleId, message.RouteName);

            // Here you can add logic to handle schedule creation
            // For example, notify customers about new routes, update caches, etc.
            
            await Task.CompletedTask;
        }
    }
}
