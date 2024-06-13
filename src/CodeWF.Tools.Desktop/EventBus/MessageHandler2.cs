namespace CodeWF.Tools.Desktop.EventBus;

//[Event]
public class MessageHandler2
{
    private readonly INotificationService _notificationService;

    public MessageHandler2(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [EventHandler]
    public void ReceiveMessage(TestMessage message)
    {
        _notificationService?.Show("CodeWF EventBus",
            $"【MessageHandler2】收到{nameof(TestMessage)}，Name: {message.Name}, Time: {message.CurrentTime}");
    }
}