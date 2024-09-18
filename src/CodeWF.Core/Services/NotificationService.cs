using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using CodeWF.Core.IServices;

namespace CodeWF.Core.Services;

public class NotificationService : INotificationService
{
    private WindowNotificationManager? _notificationManager;

    public void SetHostWindow(TopLevel level)
    {
        _notificationManager = new WindowNotificationManager(level)
        {
            Position = NotificationPosition.BottomRight, MaxItems = 4, Margin = new Avalonia.Thickness(0, 0, 15, 40)
        };
    }

    public void Show(string title, string message, NotificationType type = NotificationType.Information)
    {
        _notificationManager?.Show(new Notification(title, message, type, TimeSpan.FromSeconds(10)));
    }
}