using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace EmailService
{
    public class EmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task SendEmail(Guid notificationId, string email, string body)
        {
            _logger.LogInformation("Process send email {notificationId}, {email}", notificationId, email);

            Task.Delay(1000);

            _logger.LogInformation("Process send email.");

            return Task.CompletedTask;
        }
    }
}
