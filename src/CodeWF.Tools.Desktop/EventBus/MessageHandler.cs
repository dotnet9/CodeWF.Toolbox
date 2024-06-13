namespace CodeWF.Tools.Desktop.EventBus;

[Event]
public class MessageHandler
{
    private readonly INotificationService _notificationService;

    public MessageHandler(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [EventHandler]
    public void ReceiveMessage(TestMessage message)
    {
        _notificationService?.Show("CodeWF EventBus",
            $"【MessageHandler】收到{nameof(TestMessage)}，Name: {message.Name}, Time: {message.CurrentTime}");
    }
}