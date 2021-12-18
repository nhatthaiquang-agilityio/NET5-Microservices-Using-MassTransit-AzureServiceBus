using System;

namespace Messages.Commands
{
    public interface INotification
    {
        Guid NotificationId { get; set; }

        string NotificationType { get; set; }

        string NotificationContent { get; set; }

        string NotificationAddress { get; set; }

        DateTime NotificationDate { get; set; }
    }
}
