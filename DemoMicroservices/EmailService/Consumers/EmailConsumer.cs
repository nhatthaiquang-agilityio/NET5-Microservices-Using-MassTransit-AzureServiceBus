using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;


namespace EmailService.Consumers
{
    public class EmailConsumer : IConsumer<Messages.Commands.INotification>
    {
        private readonly ILogger<EmailConsumer> _logger;
        private readonly EmailService _emailService;
        public EmailConsumer(ILogger<EmailConsumer> logger, EmailService emailService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        }

        public Task Consume(ConsumeContext<Messages.Commands.INotification> context)
        {
            var data = context.Message;

            _logger.LogInformation(
                "Consume Email Message: {NotificationId}, {NotificationType}, {NotificationContent}, {NotificationAddress}",
                data.NotificationId, data.NotificationType, data.NotificationContent, data.NotificationAddress);

            try
            {
                // TODO: call servive/task
                Task.Delay(2000);
                _emailService.SendEmail(data.NotificationId, data.NotificationAddress, data.NotificationContent);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Unable to send Email. CorrelationId {NotificationId}, {NotificationType}, {NotificationContent}",
                    data.NotificationId, data.NotificationType, data.NotificationContent);
            }

            _logger.LogInformation("Consumed Email Message");

            return Task.CompletedTask;
        }

    }
}