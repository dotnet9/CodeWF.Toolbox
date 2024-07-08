using CodeWF.Tools.EventBus.Commands;
using CodeWF.Tools.EventBus.Queries;

namespace CodeWF.Tools.Module.Test.ViewModels;

public class EventBusTestViewModel : ViewModelBase
{
    private readonly IEventBus _eventBus;
    private readonly INotificationService _notificationService;

    public EventBusTestViewModel(INotificationService notificationService, IEventBus eventBus)
    {
        _notificationService = notificationService;
        _eventBus = eventBus;

        _eventBus.Subscribe(this);
    }


    public async Task ExecuteCommandAsync()
    {
        await _eventBus.PublishAsync(new TestCommand(nameof(EventBusTestViewModel), DateTimeOffset.Now.ToString()));
    }

    public async Task ExecuteQueryAsync()
    {
        try
        {
            var result = await _eventBus.QueryAsync(new TestQuery());
            _notificationService?.Show("CodeWF.EventBus",
                $"模块【Test】收到Query结果：{result}");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }


    [EventHandler]
    private void ReceiveCommandMessage(TestCommand message)
    {
        _notificationService?.Show("CodeWF.EventBus",
            $"模块【Test】收到{nameof(TestCommand)}，Name: {message.Name}, Time: {message.CurrentTime}");
    }

    public Task ExecuteThrowExceptionAsync()
    {
        throw new Exception("这是测试抛出的异常");
    }

    public Task ExecuteDependentLibAsync()
    {
        _notificationService?.Show("模块调用依赖", $"从依赖库得到的时间：{DateTimeOffset.Now.ToString()}");
        return Task.CompletedTask;
    }
}