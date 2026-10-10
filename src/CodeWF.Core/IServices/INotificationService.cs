using global::Avalonia.Controls;
using global::Avalonia.Controls.Notifications;

namespace CodeWF.Core.IServices;

public interface INotificationService
{
    void SetHostWindow(TopLevel level);
    void Show(string title, string message, NotificationType type = NotificationType.Information);
}