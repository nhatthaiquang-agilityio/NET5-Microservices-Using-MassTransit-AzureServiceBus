using System;

namespace Messages.Models
{
    public class NotificationViewModel
    {
        public Guid NotificationId { get; set; }

        public string NotificationType { get; set; }

        public string NotificationContent { get; set; }

        public string NotificationAddress { get; set; }

        public DateTime NotificationDate { get; set; }
    }
}
