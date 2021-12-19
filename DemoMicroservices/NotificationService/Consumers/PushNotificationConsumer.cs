using MassTransit;
using Messages.Commands;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;


namespace NotificationService.Consumers
{
    public class PushNotificationConsumer : IConsumer<INotification>
    {
        private readonly ILogger<PushNotificationConsumer> _logger;

        public PushNotificationConsumer(ILogger<PushNotificationConsumer> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task Consume(ConsumeContext<INotification> context)
        {
            var data = context.Message;

            _logger.LogInformation(
                "Consume Notification Message: {NotificationId}, {NotificationType}, {NotificationContent}",
                data.NotificationId, data.NotificationType, data.NotificationContent);

            try
            {
                // TODO: call servive/task
                Task.Delay(2000);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Unable to Notification. CorrelationId {NotificationId}, {NotificationType}, {NotificationContent}",
                    data.NotificationId, data.NotificationType, data.NotificationContent);
            }

            _logger.LogInformation("Consumed Notification Message");

            return Task.CompletedTask;
        }

    }
}
