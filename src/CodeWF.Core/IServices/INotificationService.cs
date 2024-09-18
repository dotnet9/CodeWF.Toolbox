using Avalonia.Controls;
using Avalonia.Controls.Notifications;

namespace CodeWF.Core.IServices;

public interface INotificationService
{
    void SetHostWindow(TopLevel level);
    void Show(string title, string message, NotificationType type = NotificationType.Information);
}