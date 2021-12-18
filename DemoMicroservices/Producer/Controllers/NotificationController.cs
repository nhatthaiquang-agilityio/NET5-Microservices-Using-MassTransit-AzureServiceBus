using MassTransit;
using Messages.Commands;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Producer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly ILogger<NotificationController> _logger;
        private readonly IBus _bus;

        private readonly IConfiguration _configuration;

        public NotificationController(IConfiguration configuration, ILogger<NotificationController> logger, IBus bus)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        [HttpGet]
        public IEnumerable<string> Get()
        {
            _logger.LogInformation("Get Notification API");
            return new List<string>();
        }

        [HttpPost]
        public async Task<IActionResult> Notification(Messages.Models.NotificationViewModel notificationModel)
        {
            _logger.LogInformation("Post Notification API");

            if (notificationModel != null)
            {
                var notify = new
                {
                    NotificationId = Guid.NewGuid(),
                    NotificationType = notificationModel.NotificationType,
                    NotificationDate = DateTime.Now,
                    NotificationContent = notificationModel.NotificationContent,
                    NotificationAddress = notificationModel.NotificationAddress
                };

                try
                {
                    await _bus.Publish<INotification>(notify, e =>
                    {
                        e.Headers.Set("NotificationType", notificationModel.NotificationType);
                    });

                    _logger.LogInformation(
                       "Send to a message {NotificationId}, {NotificationType}, {NotificationContent}",
                       notify.NotificationId, notify.NotificationType, notify.NotificationContent);

                    return Ok("Notification is sent.");
                }
                catch (Exception exception)
                {
                    _logger.LogError(
                        exception,
                        "Error Send Notification {NotificationId}, {NotificationType}, {NotificationContent}",
                        notify.NotificationId, notify.NotificationType, notify.NotificationContent);
                }
            }

            return BadRequest();
        }
    }
}
