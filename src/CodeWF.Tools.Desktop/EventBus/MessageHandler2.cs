using CodeWF.Tools.EventBus.Commands;

namespace CodeWF.Tools.Desktop.EventBus;

//[Event]
public class MessageHandler2
{
    private readonly INotificationService _notificationService;

    private MessageHandler2(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [EventHandler]
    private void ReceiveMessage(TestCommand message)
    {
        _notificationService?.Show("CodeWF EventBus",
            $"【MessageHandler2】收到{nameof(TestCommand)}，Name: {message.Name}, Time: {message.CurrentTime}");
    }
}