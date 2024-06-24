using CodeWF.Tools.EventBus.Commands;
using CodeWF.Tools.EventBus.Queries;

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
    private void ReceiveMessage(TestCommand message)
    {
        _notificationService?.Show("CodeWF EventBus",
            $"【MessageHandler】收到{nameof(TestCommand)}，Name: {message.Name}, Time: {message.CurrentTime}");
    }


    [EventHandler]
    private async Task ReceiveQuery(TestQuery query)
    {
        // TODO EventBus BUG
        //await Task.Delay(TimeSpan.FromSeconds(1));    
        query.Result = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff");
    }
}